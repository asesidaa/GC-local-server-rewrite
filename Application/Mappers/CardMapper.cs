using Application.Dto;
using Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Application.Mappers;

[Mapper]
public static partial class CardMapper
{
    public static partial CardDto CardMainToCardDto(this CardMain cardMain);
}