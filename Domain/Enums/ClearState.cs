using NetEscapades.EnumGenerators;

namespace Domain.Enums;

[EnumExtensions]
public enum ClearState
{
    NotPlayed = 0,
    Failed,
    Clear,
    NoMiss,
    FullChain,
    Perfect
}