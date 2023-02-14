using Application.Dto;
using Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Application.Mappers;

[Mapper]
public static partial class PlayNumRankMapper
{
    [MapProperty(nameof(PlayNumRank.MusicId), nameof(PlayNumRankDto.Pcol1))]
    [MapProperty(nameof(PlayNumRank.PlayCount), nameof(PlayNumRankDto.ScoreBi1))]
    public static partial PlayNumRankDto PlayNumRankToDto(this PlayNumRank rank);
}