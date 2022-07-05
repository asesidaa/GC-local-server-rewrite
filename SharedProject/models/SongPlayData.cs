using System.Text.Json.Serialization;
using SharedProject.common;
using SharedProject.enums;

namespace SharedProject.models;

public class SongPlayData
{
    public string Title { get; set; } = string.Empty;

    public string Artist { get; set; } = string.Empty;
    
    public int MusicId { get; set; }

    public SongPlayDetailData[] SongPlaySubDataList { get; set; } = new SongPlayDetailData[SharedConstants.DIFFICULTY_COUNT];

    public bool IsFavorite { get; set; }
    
    public bool ShowDetails { get; set; }

    [JsonIgnore]
    public int TotalPlayCount
    {
        get
        {
            return SongPlaySubDataList.Sum(data => data.PlayCount);
        }
    }

    [JsonIgnore]
    public DateTime LastPlayTime
    {
        get
        {
            var songPlayDetailData = SongPlaySubDataList.Where(data => data.ClearState != ClearState.NotPlayed)
                .MinBy(data => data.LastPlayTime);
            return songPlayDetailData?.LastPlayTime ?? DateTime.MaxValue;
        }
    }
}