using Application.Api;
using Microsoft.AspNetCore.Mvc;
using Shared.Dto.Api;
using Shared.Models;

namespace MainServer.Controllers.API;

[ApiController]
[Route("api/[controller]")]
public class ProfilesController : BaseController<ProfilesController>
{
    [HttpGet]
    public async Task<ActionResult<ServiceResult<List<ClientCardDto>>>> GetAllCards()
    {
        var result = await Mediator.Send(new GetCardsQuery());
        return result;
    }

    [HttpGet("{cardId:long}")]
    public async Task<ActionResult<ServiceResult<TotalResultData>>> GetCardTotalResultById(long cardId)
    {
        var result = await Mediator.Send(new GetTotalResultQuery(cardId));
        return result;
    }

    [HttpPost("Favorite")]
    public async Task<ActionResult<ServiceResult<bool>>> SetFavoriteMusic(MusicDetailDto detail)
    {
        var result = await Mediator.Send(new SetFavoriteMusicCommand(detail));
        return result;
    }

    [HttpPost("PlayerName")]
    public async Task<ActionResult<ServiceResult<bool>>> SetPlayerName(ClientCardDto card)
    {
        var result = await Mediator.Send(new SetPlayerNameCommand(card));
        return result;
    }
}