using Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Application.Mappers;

[Mapper]
public static partial class MusicMapper
{
    public static partial MusicDto MusicToDto(this MusicUnlock music);

    private static int BoolToInt(bool value) => value ? 1 : 0;
}