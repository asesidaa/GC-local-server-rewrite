using Application.Dto;
using Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Application.Mappers;

[Mapper]
public static partial class CardBDatumMapper
{
    public static partial CardBDatumDto CardBDatumToDto(this CardBdatum cardBdatum);
}