using Microsoft.AspNetCore.Mvc;

namespace MainServer.Controllers.Game;

[ApiController]
[Route("alive")]
public class AliveController : ControllerBase
{
    [HttpGet("i.php")]
    public IActionResult AliveCheck()
    {
        var remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress;
        var serverIpAddress = Request.HttpContext.Connection.LocalIpAddress;
        var response = $"REMOTE ADDRESS:{remoteIpAddress}\n" +
                       "SERVER NAME:GCLocalServer\n" +
                       $"SERVER ADDR:{serverIpAddress}";
        return Ok(response);
    }

    [HttpGet("/{id}/Alive.txt")]
    public IActionResult GetAliveFile()
    {
        return Ok("");
    }
}