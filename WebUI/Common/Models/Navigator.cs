namespace WebUI.Common.Models;

public class Navigator
{
    public uint Id { get; set; }

    public string NavigatorName { get; set; } = string.Empty;

    public NavigatorGenre Genre { get; set; }

    public string IllustrationCredit { get; set; } = string.Empty;

    public string ToolTipJp { get; set; } = string.Empty;

    public string ToolTipEn { get; set; } = string.Empty;
}

public enum NavigatorGenre
{
    Default  = 1,
    Original = 2,
    Game     = 3,
    Touhou   = 4,
    Vocaloid = 5,
    Collab   = 6,
}