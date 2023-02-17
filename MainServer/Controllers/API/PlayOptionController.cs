using Application.Api;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace MainServer.Controllers.API;

[ApiController]
[Route("api/[controller]")]
public class PlayOptionController : BaseController<PlayOptionController>
{
    [HttpGet("{cardId:long}")]
    public async Task<ActionResult<ServiceResult<PlayOptionData>>> GetPlayOptionById(long cardId)
    {
        var result = await Mediator.Send(new GetPlayOptionQuery(cardId));
        return result;
    }

    [HttpPost]
    public async Task<ActionResult<ServiceResult<bool>>> SetPlayOption(PlayOptionData data)
    {
        var result = await Mediator.Send(new SetPlayOptionCommand(data));
        return result;
    }
}