using ProtoBuf;

namespace SharedProject.enums;

[ProtoContract]
public enum NavigatorGenre
{
    Default = 1,
    Original = 2,
    Game = 3,
    Touhou = 4,
    Vocaloid = 5,
    Collab = 6,
}