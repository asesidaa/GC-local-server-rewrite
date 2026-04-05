namespace Domain.Entities;

public class PlayerCoin
{
    public long CardId { get; set; }

    public int CurrentCoins { get; set; }

    public int TotalCoins { get; set; }

    public int MonthlyCoins { get; set; }
}
