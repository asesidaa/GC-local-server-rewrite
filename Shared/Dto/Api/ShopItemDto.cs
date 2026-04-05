namespace Shared.Dto.Api;

public class ShopItemDto
{
    public int Id { get; set; }

    public string ItemType { get; set; } = string.Empty;

    public int ItemId { get; set; }

    public string ItemName { get; set; } = string.Empty;

    public int CoinCost { get; set; }
}
