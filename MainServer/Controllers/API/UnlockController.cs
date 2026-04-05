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
public class UnlockController : BaseController<UnlockController>
{
    [HttpGet("{cardId:long}")]
    public async Task<ServiceResult<List<UnlockStateDto>>> GetUnlockState(long cardId)
    {
        return await Mediator.Send(new GetUnlockStateQuery(cardId));
    }

    [HttpPost("{cardId:long}/{itemType}/{itemId:int}")]
    public async Task<ServiceResult<bool>> UnlockItem(long cardId, UnlockItemType itemType, int itemId)
    {
        return await Mediator.Send(new SetUnlockCommand(cardId, itemType, itemId, true));
    }

    [HttpDelete("{cardId:long}/{itemType}/{itemId:int}")]
    public async Task<ServiceResult<bool>> LockItem(long cardId, UnlockItemType itemType, int itemId)
    {
        return await Mediator.Send(new SetUnlockCommand(cardId, itemType, itemId, false));
    }

    [HttpPost("{cardId:long}/{itemType}/all")]
    public async Task<ServiceResult<bool>> UnlockAll(long cardId, UnlockItemType itemType)
    {
        return await Mediator.Send(new SetUnlockAllCommand(cardId, itemType, true));
    }

    [HttpDelete("{cardId:long}/{itemType}/all")]
    public async Task<ServiceResult<bool>> LockAll(long cardId, UnlockItemType itemType)
    {
        return await Mediator.Send(new SetUnlockAllCommand(cardId, itemType, false));
    }
}
