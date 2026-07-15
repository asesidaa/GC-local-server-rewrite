using System.Security.Claims;
using Application.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Dto.Api;
using Shared.Models;

namespace MainServer.Controllers.API;

[ApiController]
[Route("api/[controller]")]
public class ProfilesController : BaseController<ProfilesController>
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ServiceResult<List<ClientCardDto>>> GetAllCards()
    {
        var result = await Mediator.Send(new GetCardsQuery());
        return result;
    }

    [HttpGet("TotalResult/{cardId:long}")]
    [AllowAnonymous]
    public async Task<ServiceResult<TotalResultData>> GetCardTotalResultById(long cardId)
    {
        var result = await Mediator.Send(new GetTotalResultQuery(cardId));
        return result;
    }

    [HttpGet("SongPlayRecords/{cardId:long}")]
    [AllowAnonymous]
    public async Task<ServiceResult<List<SongPlayRecord>>> GetSongPlayRecords(long cardId)
    {
        var result = await Mediator.Send(new GetSongPlayRecordsQuery(cardId));
        return result;
    }

    [HttpPost("SetFavorite")]
    [Authorize]
    public async Task<ServiceResult<bool>> SetFavoriteMusic(MusicFavoriteDto favorite)
    {
        if (!IsOwnerOrAdmin(favorite.CardId))
            return ServiceResult.Failed<bool>(ServiceError.CustomMessage("You can only modify your own card"));

        var result = await Mediator.Send(new SetFavoriteMusicCommand(favorite));
        return result;
    }

    [HttpPost("PlayerName")]
    [Authorize]
    public async Task<ServiceResult<bool>> SetPlayerName(ClientCardDto card)
    {
        if (!IsOwnerOrAdmin(card.CardId))
            return ServiceResult.Failed<bool>(ServiceError.CustomMessage("You can only modify your own card"));

        var result = await Mediator.Send(new SetPlayerNameCommand(card));
        return result;
    }

    [HttpPost("UnlockAllMusic/{cardId:long}")]
    [Authorize(Roles = "Admin")]
    public async Task<ServiceResult<bool>> UnlockAllMusic(long cardId)
    {
        var result = await Mediator.Send(new UnlockAllMusicCommand(cardId));
        return result;
    }

    private bool IsOwnerOrAdmin(long cardId)
    {
        var role = HttpContext.User.FindFirstValue(ClaimTypes.Role);
        if (role == "Admin") return true;

        var cardIdStr = HttpContext.User.FindFirstValue("CardId");
        return long.TryParse(cardIdStr, out var ownCardId) && ownCardId == cardId;
    }
}
