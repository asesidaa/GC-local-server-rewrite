namespace Domain.Entities;

public partial class MusicUnlock
{
    public long MusicId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Artist { get; set; } = string.Empty;

    public DateTime ReleaseDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool NewFlag { get; set; }

    public bool UseFlag { get; set; }
}
