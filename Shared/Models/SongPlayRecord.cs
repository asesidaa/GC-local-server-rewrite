using System.Text.Json.Serialization;

namespace Shared.Models;

public class SongPlayRecord
{
    public string Title { get; set; } = string.Empty;

    public string Artist { get; set; } = string.Empty;
    
    public int MusicId { get; set; }
    
    public bool IsFavorite { get; set; }

    public int TotalPlayCount { get; set; }

    public List<StagePlayRecord> StagePlayRecords { get; set; } = new();
}