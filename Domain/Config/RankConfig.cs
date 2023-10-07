namespace Domain.Config;

public class RankConfig
{
    public const string RANK_SECTION = "Rank";
    
    public int RefreshIntervalHours { get; set; } = 24;
}