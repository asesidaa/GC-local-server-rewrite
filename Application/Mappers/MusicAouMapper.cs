﻿using Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Application.Mappers;

[Mapper]
public static partial class MusicAouMapper
{
    public static partial MusicAouDto MusicAouToDto(this MusicAou musicAou);
    
    private static int BoolToInt(bool value) => value ? 1 : 0;
}