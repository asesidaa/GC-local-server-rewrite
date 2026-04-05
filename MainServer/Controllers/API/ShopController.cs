using System.Security.Claims;
using Application.Api;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Dto.Api;
using Shared.Models;

namespace MainServer.Controllers.API;

[ApiController]
[Route("api/[controller]")]
public class ShopController : BaseController<ShopController>
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ServiceResult<List<ShopItemDto>>> GetShopItems()
    {
        return await Mediator.Send(new GetShopItemsQuery());
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ServiceResult<bool>> AddShopItem(ShopItemDto dto)
    {
        if (!Enum.TryParse<UnlockItemType>(dto.ItemType, out var itemType))
            return ServiceResult.Failed<bool>(ServiceError.CustomMessage($"Invalid item type: {dto.ItemType}"));

        return await Mediator.Send(new AddShopItemCommand(itemType, dto.ItemId, dto.CoinCost));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ServiceResult<bool>> RemoveShopItem(int id)
    {
        return await Mediator.Send(new RemoveShopItemCommand(id));
    }

    [HttpPost("Purchase")]
    [Authorize]
    public async Task<ServiceResult<PlayerCoinDto>> PurchaseItem(PurchaseRequest request)
    {
        var role = HttpContext.User.FindFirstValue(ClaimTypes.Role);
        if (role != "Admin")
        {
            var cardIdStr = HttpContext.User.FindFirstValue("CardId");
            if (!long.TryParse(cardIdStr, out var ownCardId) || ownCardId != request.CardId)
                return ServiceResult.Failed<PlayerCoinDto>(
                    ServiceError.CustomMessage("You can only purchase for your own card"));
        }

        return await Mediator.Send(new PurchaseItemCommand(request.CardId, request.ShopItemId));
    }
}
