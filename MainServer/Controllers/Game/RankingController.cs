using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Throw;

namespace MainServer.Controllers.Game;

[ApiController]
[Route("ranking")]
public class RankingController : BaseController<RankingController>
{
    [HttpGet("ranking.php")]
    public async Task<ActionResult<string>> Ranking([FromQuery(Name = "cmd_type")] int rankType)
    {
        var type = (RankingCommandType)rankType;
        type.Throw().IfOutOfRange();

        switch (type)
        {
            case RankingCommandType.GlobalRank:
                break;
            case RankingCommandType.PlayNumRank:
                break;
            case RankingCommandType.EventRank:
                break;
            case RankingCommandType.MonthlyRank:
                break;
            case RankingCommandType.ShopRank:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, "Should not happen!");
        }
        return "";
    }
}