namespace Domain.Entities;

public class DefaultUnlockState
{
    public int ItemType { get; set; }

    public string UnlockedBitset { get; set; } = "[]";
}
