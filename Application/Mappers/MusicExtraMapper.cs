using Application.Dto;
using Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Application.Mappers;

[Mapper]
public static partial class MusicExtraMapper
{
    public static partial MusicExtraDto MusicExtraToDto(this MusicExtra musicExtra);
    
    private static int BoolToInt(bool value) => value ? 1 : 0;
}