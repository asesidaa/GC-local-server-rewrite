using Microsoft.AspNetCore.Mvc;

namespace MainServer.Controllers.Game;

[ApiController]
[Route("service/incom")]
public class IncomController : ControllerBase
{
    private const string INCOM_RESPONSE = "1+1";
    
    [HttpPost("incom.php")]
    public IActionResult Incom()
    {
        return Ok(INCOM_RESPONSE);
    }
    
    [HttpPost("incomALL.php")]
    public IActionResult IncomAll()
    {
        return Ok(INCOM_RESPONSE);
    }
}