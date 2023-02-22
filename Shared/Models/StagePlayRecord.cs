using Domain.Enums;

namespace Shared.Models;

public class StagePlayRecord
{
    public int Score { get; set; }
    
    public int PlayCount { get; set; }
    
    public int MaxChain { get; set; }
    
    public Difficulty Difficulty { get; set; }
    
    public ClearState ClearState { get; set; }
    
    public DateTime LastPlayTime { get; set; }
}