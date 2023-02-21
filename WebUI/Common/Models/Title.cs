namespace WebUI.Common.Models;

public class Title
{
    public uint Id { get; set; }

    public string TitleName { get; set; } = string.Empty;

    public string UnlockRequirementJp { get; set; } = string.Empty;
    
    public string UnlockRequirementEn { get; set; } = string.Empty;

    public UnlockType UnlockType { get; set; }
}

public enum UnlockType
{
    Invalid               = 0,
    Default               = 1,
    Clear                 = 2,
    NoMiss                = 3,
    FullChain             = 4,
    SRankSimpleStages     = 5,
    SRankNormalStages     = 6,
    SRankHardStages       = 7,
    SRankExtraStages      = 8,
    SRankSNH              = 9,
    SPlusRankSNH          = 10,
    SPlusPlusRankSNH      = 11,
    Event                 = 12,
    Prefecture            = 13,
    ChainMilestone        = 14,
    Adlibs                = 15,
    ConsecutiveNoMiss     = 16,
    ClearsUsingItems      = 17,
    Avatars               = 18,
    MultiplayerStarsTotal = 19,
    SongSet20             = 20,
    SongSet21             = 21,
    SongSet22             = 22,
    SongSet23             = 23,
    SongSet24             = 24,
    SongSet25             = 25,
    SongSet26             = 26,
    ProfileLevel          = 27,
    Perfect               = 28,
    OnlineMatching        = 29,
    Trophies              = 30,
}