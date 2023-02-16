using Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Application.Mappers;

[Mapper]
public static partial class ScoreRankMapper
{
    public static partial ScoreRankDto ScoreRankToDto(this ScoreRank rank);
}