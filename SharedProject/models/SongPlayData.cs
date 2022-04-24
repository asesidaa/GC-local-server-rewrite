namespace SharedProject.models;

public class SongPlayData
{
    public string Title { get; set; } = string.Empty;

    public string Artist { get; set; } = string.Empty;

    public List<SongPlayDetailData> SongPlaySubDataList { get; set; } = new List<SongPlayDetailData>(4);
    
    public DateTime LastPlayTime { get; set; }
    
    public int TotalPlayCount { get; set; }
    
    public bool ShowDetails { get; set; }
}