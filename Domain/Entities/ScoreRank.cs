namespace Domain.Entities;

public class ScoreRank
{
    public long CardId { get; set; }

    public string PlayerName { get; set; } = string.Empty;

    public long Rank { get; set; }

    public long TotalScore { get; set; }
    
    public int AvatarId { get; set; }

    public long TitleId { get; set; }

    public long Fcol1 { get; set; }

    public int PrefId { get; set; }

    public string Pref { get; set; } = string.Empty;

    public int AreaId { get; set; }

    public string Area { get; set; } = string.Empty;

    public int LastPlayTenpoId { get; set; }

    public string TenpoName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    
}