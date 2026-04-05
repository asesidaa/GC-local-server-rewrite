namespace Domain.Entities;

public class PlayerUnlockState
{
    public long CardId { get; set; }

    public int ItemType { get; set; }

    public string UnlockedBitset { get; set; } = "[]";
}
