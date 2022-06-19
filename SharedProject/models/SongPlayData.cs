namespace SharedProject.models;

public class SongPlayData
{
    public string Title { get; set; } = string.Empty;

    public string Artist { get; set; } = string.Empty;

    public SongPlayDetailData[] SongPlaySubDataList { get; set; } = new SongPlayDetailData[4];

    public bool IsFavorite { get; set; }
    
    public bool ShowDetails { get; set; }
}