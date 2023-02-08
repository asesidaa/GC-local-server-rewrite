using Microsoft.AspNetCore.Mvc;

namespace MainServer.Controllers.Game;

[ApiController]
[Route("update/cgi")]
public class UpdateController : ControllerBase
{
    // TODO: Check update properly
    [HttpGet("check.php")]
    public IActionResult UpdateCheck()
    {
        return NotFound();
    }
}