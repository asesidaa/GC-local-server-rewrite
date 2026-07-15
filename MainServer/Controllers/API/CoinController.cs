using System.Security.Claims;
using Application.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Dto.Api;
using Shared.Models;

namespace MainServer.Controllers.API;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CoinController : BaseController<CoinController>
{
    [HttpGet("{cardId:long}")]
    public async Task<ServiceResult<PlayerCoinDto>> GetCoins(long cardId)
    {
        var role = HttpContext.User.FindFirstValue(ClaimTypes.Role);
        if (role != "Admin")
        {
            var cardIdStr = HttpContext.User.FindFirstValue("CardId");
            if (!long.TryParse(cardIdStr, out var ownCardId) || ownCardId != cardId)
                return ServiceResult.Failed<PlayerCoinDto>(
                    ServiceError.CustomMessage("You can only view your own coin balance"));
        }

        return await Mediator.Send(new GetPlayerCoinsQuery(cardId));
    }

    [HttpPost("{cardId:long}")]
    [Authorize(Roles = "Admin")]
    public async Task<ServiceResult<bool>> SetCoins(long cardId, PlayerCoinDto coins)
    {
        coins.CardId = cardId;
        return await Mediator.Send(new SetPlayerCoinsCommand(coins));
    }
}
