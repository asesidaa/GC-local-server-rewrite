using Application.Api;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Dto.Api;
using Shared.Models;

namespace MainServer.Controllers.API;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class DefaultUnlockController : BaseController<DefaultUnlockController>
{
    [HttpGet]
    public async Task<ServiceResult<List<DefaultUnlockStateDto>>> GetDefaultUnlockState()
    {
        return await Mediator.Send(new GetDefaultUnlockStateQuery());
    }

    [HttpGet("{itemType}")]
    public async Task<ServiceResult<DefaultUnlockDetailDto>> GetDefaultUnlockBitset(UnlockItemType itemType)
    {
        return await Mediator.Send(new GetDefaultUnlockBitsetQuery(itemType));
    }

    [HttpPost("{itemType}/{itemId:int}")]
    public async Task<ServiceResult<bool>> SetDefaultUnlocked(UnlockItemType itemType, int itemId)
    {
        return await Mediator.Send(new SetDefaultUnlockCommand(itemType, itemId, true));
    }

    [HttpDelete("{itemType}/{itemId:int}")]
    public async Task<ServiceResult<bool>> SetDefaultLocked(UnlockItemType itemType, int itemId)
    {
        return await Mediator.Send(new SetDefaultUnlockCommand(itemType, itemId, false));
    }
}
