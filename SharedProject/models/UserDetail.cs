namespace SharedProject.models;

public class UserDetail: User
{
    public PlayOption PlayOption { get; set; } = new PlayOption();
    
    public long TotalScore { get; set; }
    
    public int AverageScore { get; set; }
    
    public int TotalSongCount { get; set; }
    
    public int PlayedSongCount { get; set; }
    
    public int TotalStageCount { get; set; }
    
    public int ClearedStageCount { get; set; }
    
    public int NoMissStageCount { get; set; }
    
    public int FullChainStageCount { get; set; }
    
    public int PerfectStageCount { get; set; }
    
    public int SAboveStageCount { get; set; }
    
    public int SPlusAboveStageCount { get; set; }
    
    public int SPlusPlusAboveStageCount { get; set; }
}