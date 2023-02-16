using Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Application.Mappers;

[Mapper]
public static partial class CardBDatumMapper
{
    [MapProperty(nameof(CardBdatum.Bdata), nameof(CardBDatumDto.CardBdata))]
    [MapProperty(nameof(CardBdatum.BdataSize), nameof(CardBDatumDto.BDataSize))]
    public static partial CardBDatumDto CardBDatumToDto(this CardBdatum cardBdatum);
    
    [MapProperty(nameof(CardBDatumDto.CardBdata), nameof(CardBdatum.Bdata))]
    [MapProperty(nameof(CardBDatumDto.BDataSize), nameof(CardBdatum.BdataSize))]
    public static partial CardBdatum DtoToCardBDatum(this CardBDatumDto dto);
}