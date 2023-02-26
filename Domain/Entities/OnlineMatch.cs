namespace Domain.Entities;

public class OnlineMatch
{
    public long MatchId { get; set; }

    public List<OnlineMatchEntry> Entries { get; set; } = new();

    public bool IsOpen { get; set; }

    public Guid Guid { get; set; }
}