using ProtoBuf;

namespace SharedProject.enums;

[ProtoContract]
public enum TitleUnlockType
{
    Invalid = 0,
    Default = 1,
    Clear = 2,
    NoMiss = 3,
    FullChain = 4,
    SRankSimpleStages = 5,
    SRankNormalStages = 6,
    SRankHardStages = 7,
    SRankExtraStages = 8,
    SRankAllDifficulties = 9,
    SPlusRankAllDifficulties = 10,
    SPlusPlusRankAllDifficulties  = 11,
    Event = 12,
    Prefecture = 13,
    ChainMilestone = 14,
    Adlibs = 15,
    ConsecutiveNoMiss = 16,
    ClearsUsingItems = 17,
    Avatars = 18,
    MultiplayerStarsTotal = 19,
    SongSet20 = 20,
    SongSet21 = 21,
    SongSet22 = 22,
    SongSet23 = 23,
    SongSet24 = 24,
    SongSet25 = 25,
    SongSet26 = 26,
    ProfileLevel = 27,
    Perfect = 28,
    OnlineMatching = 29,
    Trophies = 30,
}