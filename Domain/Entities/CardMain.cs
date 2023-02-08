namespace Domain.Entities;

public partial class CardMain
{
    public long CardId { get; set; }

    public string PlayerName { get; set; } = string.Empty;

    public long ScoreI1 { get; set; }

    public long Fcol1 { get; set; }

    public long Fcol2 { get; set; }

    public long Fcol3 { get; set; }

    public string AchieveStatus { get; set; } = string.Empty;

    public string? Created { get; set; } = string.Empty;

    public string? Modified { get; set; } = string.Empty;
}
