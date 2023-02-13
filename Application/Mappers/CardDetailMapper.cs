using Application.Dto;
using Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Application.Mappers;

[Mapper]
public static partial class CardDetailMapper
{
    public static partial CardDetailDto CardDetailToDto(this CardDetail cardDetail);

    public static partial CardDetail DtoToCardDetail(this CardDetailDto dto);
}