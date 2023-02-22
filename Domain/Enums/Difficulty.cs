using NetEscapades.EnumGenerators;

namespace Domain.Enums;

[EnumExtensions]
public enum Difficulty
{
    Simple = 0,
    Normal = 1,
    Hard   = 2,
    Extra  = 3
}