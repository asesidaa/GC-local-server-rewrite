using System.ComponentModel.DataAnnotations;

namespace Domain.Config;

public sealed class OnlineMatchingConfig
{
    public const string SECTION = "OnlineMatching";

    [Range(1, int.MaxValue)]
    public int SearchTimeoutSeconds { get; set; } = 99;

    [Range(1, int.MaxValue)]
    public int FormationTimeoutSeconds { get; set; } = 40;

    [Range(1, int.MaxValue)]
    public int PollIntervalSeconds { get; set; } = 10;

    [Range(1, int.MaxValue)]
    public int RoomIdleExpirySeconds { get; set; } = 600;

    [Range(1, int.MaxValue)]
    public int TerminalRetentionSeconds { get; set; } = 120;

    [Range(1, int.MaxValue)]
    public int MatchingIdReuseGuardSeconds { get; set; } = 600;

    [Range(1, 4)]
    public int MaxPlayers { get; set; } = 4;
}
