namespace Application.Common.Models;

public class TotalResultData
{
    public long CardId { get; set; }

    public string PlayerName { get; set; } = string.Empty;
    
    public PlayerData PlayerData { get; set; } = new();

    public StageCountData StageCountData { get; set; } = new();
}

public class PlayerData
{
    public long TotalScore { get; set; }
    
    public int AverageScore { get; set; }
    
    public int TotalSongCount { get; set; }
    
    public int PlayedSongCount { get; set; }

    public int Rank { get; set; }
}

public class StageCountData
{
    public int Total { get; set; }
    
    public int Cleared { get; set; }
    
    public int NoMiss { get; set; }
    
    public int FullChain { get; set; }
    
    public int S { get; set; }
    
    public int Ss { get; set; }
    
    public int Sss { get; set; }
    
    public int Perfect { get; set; }
}