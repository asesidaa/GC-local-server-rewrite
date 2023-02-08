using Microsoft.AspNetCore.Mvc;

namespace MainServer.Controllers.Game;

[ApiController]
[Route("service/respone")]
public class ResponeController : ControllerBase
{
    [HttpPost("respone.php")]
    public IActionResult Respone()
    {
        return Ok("1");
    }
}