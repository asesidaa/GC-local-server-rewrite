using SharedProject.enums;

namespace SharedProject.models;

public class SongPlayDetailData
{
    public int Score { get; set; }
    
    public int PlayCount { get; set; }
    
    public int MaxChain { get; set; }
    
    public Difficulty Difficulty { get; set; }
    
    public ClearState ClearState { get; set; }
    
    public DateTime LastPlayTime { get; set; }
}