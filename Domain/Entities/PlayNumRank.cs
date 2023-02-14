namespace Domain.Entities;

public class PlayNumRank
{
    public int MusicId { get; set; }
    
    public int PlayCount { get; set; }
    
    public int Rank { get; set; }
    
    public int Rank2 { get; set; }
    
    public int PrevRank { get; set; }
    public int PrevRank2 { get; set; }

    public string Title { get; set; } = string.Empty;
    
    public string Artist { get; set; } = string.Empty;
}