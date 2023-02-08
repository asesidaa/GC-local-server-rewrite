namespace Domain.Entities;

public partial class CardPlayCount
{
    public long CardId { get; set; }

    public long PlayCount { get; set; }

    public DateTime LastPlayedTime { get; set; }
}
