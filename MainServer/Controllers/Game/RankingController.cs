using Application.Common.Models;
using Application.Game.Rank;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Throw;

namespace MainServer.Controllers.Game;

[ApiController]
[Route("ranking")]
public class RankingController : BaseController<RankingController>
{
    [HttpGet("ranking.php")]
    public async Task<ActionResult<string>> Ranking([FromQuery(Name = "cmd_type")] int rankType,
        [FromQuery(Name = "tenpo_id")] int tenpoId)
    {
        var type = (RankingCommandType)rankType;
        type.Throw().IfOutOfRange();

        var result = type switch
        {
            RankingCommandType.GlobalRank => await Mediator.Send(new GetGlobalScoreRankQuery()),
            RankingCommandType.PlayNumRank => await Mediator.Send(new GetPlayNumRankQuery()),
            RankingCommandType.EventRank => await Mediator.Send(new GetEventRankQuery()),
            RankingCommandType.MonthlyRank => await Mediator.Send(new GetMonthlyScoreRankQuery()),
            RankingCommandType.ShopRank => await Mediator.Send(new GetTenpoScoreRankQuery(tenpoId)),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Should not happen!")
        };

        if (result.Succeeded)
        {
            var normalResult = "1\n" +
                               $"{result.Data}";
            return Ok(normalResult);
        }

        // Here error is not null since Succeeded => Error is null; 
        var errorMessage = $"{result.Error!.Code}\n" +
                           $"{result.Error!.Message}";
        return Ok(errorMessage);
    }
}