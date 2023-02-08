using Application.Game.Option;
using Microsoft.AspNetCore.Mvc;

namespace MainServer.Controllers.Game;

[ApiController]
[Route("service/option")]
public class ServiceOptionController : BaseController<ServiceOptionController>
{
    [HttpGet("PlayInfo.php")]
    public async Task<ActionResult<string>> GetPlayCount([FromQuery(Name = "card_id")] long cardId)
    {
        var query = new PlayCountQuery(cardId);
        var count = await Mediator.Send(query);

        return Ok("1\n" +
                  $"{count}");
    }
}