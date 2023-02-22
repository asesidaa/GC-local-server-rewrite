namespace Domain.Entities;

public partial class CardDetail
{
    public long CardId { get; set; }

    public long Pcol1 { get; set; }

    public long Pcol2 { get; set; }

    public long Pcol3 { get; set; }

    public long ScoreI1 { get; set; }

    public long ScoreUi1 { get; set; }

    public long ScoreUi2 { get; set; }

    public long ScoreUi3 { get; set; }

    public long ScoreUi4 { get; set; }

    public long ScoreUi5 { get; set; }

    public long ScoreUi6 { get; set; }

    public long ScoreBi1 { get; set; }

    public string? LastPlayTenpoId { get; set; } = string.Empty;

    public long Fcol1 { get; set; }

    public long Fcol2 { get; set; }

    public long Fcol3 { get; set; }

    public DateTime? LastPlayTime { get; set; }
}
