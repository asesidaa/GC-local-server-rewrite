using Microsoft.AspNetCore.Mvc;

namespace MainServer.Controllers.Game;
[ApiController]
[Route("service/upload")]
public class UploadController : ControllerBase
{
    private const string UPLOAD_RESPONSE = "1\n" +
                                           "OK";
    
    [HttpPost("upload.php")]
    public IActionResult Upload()
    {
        return Ok(UPLOAD_RESPONSE);
    }
}