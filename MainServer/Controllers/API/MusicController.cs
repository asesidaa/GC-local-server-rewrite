using Application.Api;
using Microsoft.AspNetCore.Mvc;
using Shared.Dto.Api;
using Shared.Models;

namespace MainServer.Controllers.API;

[ApiController]
[Route("api/[controller]")]
public class MusicController : BaseController<MusicController>
{
    [HttpGet]
    public async Task<ServiceResult<List<MusicItemDto>>> GetAllMusic()
    {
        return await Mediator.Send(new GetMusicListQuery());
    }
}
