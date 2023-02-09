namespace Domain.Config;

public class GameConfig
{
    public const string GAME_SECTION = "Game";

    public int AvatarCount { get; set; } 
    
    public int NavigatorCount { get; set; } 
    
    public int ItemCount { get; set; } 
    
    public int SkinCount { get; set; } 
    
    public int SeCount { get; set; } 
    
    public int TitleCount { get; set; }

    public List<int> UnlockableSongIds { get; set; } = new();
}