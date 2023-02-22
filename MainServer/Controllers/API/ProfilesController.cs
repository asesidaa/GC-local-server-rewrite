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
    public async Task<ServiceResult<List<ClientCardDto>>> GetAllCards()
    {
        var result = await Mediator.Send(new GetCardsQuery());
        return result;
    }

    [HttpGet("TotalResult/{cardId:long}")]
    public async Task<ServiceResult<TotalResultData>> GetCardTotalResultById(long cardId)
    {
        var result = await Mediator.Send(new GetTotalResultQuery(cardId));
        return result;
    }

    [HttpGet("SongPlayRecords/{cardId:long}")]
    public async Task<ServiceResult<List<SongPlayRecord>>> GetSongPlayRecords(long cardId)
    {
        var result = await Mediator.Send(new GetSongPlayRecordsQuery(cardId));

        return result;
    }

    [HttpPost("SetFavorite")]
    public async Task<ServiceResult<bool>> SetFavoriteMusic(MusicFavoriteDto favorite)
    {
        var result = await Mediator.Send(new SetFavoriteMusicCommand(favorite));
        return result;
    }

    [HttpPost("PlayerName")]
    public async Task<ServiceResult<bool>> SetPlayerName(ClientCardDto card)
    {
        var result = await Mediator.Send(new SetPlayerNameCommand(card));
        return result;
    }

    [HttpPost("UnlockAllMusic/{cardId:long}")]
    public async Task<ServiceResult<bool>> UnlockAllMusic(long cardId)
    {
        var result = await Mediator.Send(new UnlockAllMusicCommand(cardId));

        return result;
    }
}