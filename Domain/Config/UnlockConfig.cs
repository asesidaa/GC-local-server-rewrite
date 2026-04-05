namespace Domain.Config;

public class UnlockConfig
{
    public const string UNLOCK_SECTION = "Unlock";

    public bool EnableLockSystem { get; set; } = false;

    public int DefaultCoins { get; set; } = 900000;
}
