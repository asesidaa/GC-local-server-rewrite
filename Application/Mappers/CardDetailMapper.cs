using Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Application.Mappers;

[Mapper]
public static partial class CardDetailMapper
{
    public static partial CardDetailDto CardDetailToDto(this CardDetail cardDetail);

    public static partial CardDetail DtoToCardDetail(this CardDetailDto dto);

    [MapperIgnoreSource(nameof(FirstPlayOptionDto.CardId))]
    [MapProperty(nameof(FirstPlayOptionDto.AvatarId), nameof(CardDetail.ScoreI1))]
    [MapProperty(nameof(FirstPlayOptionDto.TitleId), nameof(CardDetail.Fcol2))]
    [MapProperty(nameof(FirstPlayOptionDto.ShowFastSlowOption), nameof(CardDetail.ScoreUi1))]
    [MapProperty(nameof(FirstPlayOptionDto.ShowFeverTranceOption), nameof(CardDetail.ScoreUi2))]
    public static partial void MapFirstOptionDetail(this FirstPlayOptionDto dto, CardDetail detail);
    
    [MapperIgnoreSource(nameof(SecondPlayOptionDto.CardId))]
    [MapProperty(nameof(SecondPlayOptionDto.NavigatorId), nameof(CardDetail.ScoreI1))]
    public static partial void MapSecondOptionDetail(this SecondPlayOptionDto dto, CardDetail detail);

    [MapProperty(nameof(CardDetail.ScoreI1), nameof(FirstPlayOptionDto.AvatarId))]
    [MapProperty(nameof(CardDetail.Fcol2), nameof(FirstPlayOptionDto.TitleId))]
    [MapProperty(nameof(CardDetail.ScoreUi1), nameof(FirstPlayOptionDto.ShowFastSlowOption))]
    [MapProperty(nameof(CardDetail.ScoreUi2), nameof(FirstPlayOptionDto.ShowFeverTranceOption))]
    public static partial FirstPlayOptionDto CardDetailToFirstOption(this CardDetail detail);

    [MapProperty(nameof(CardDetail.ScoreI1), nameof(SecondPlayOptionDto.NavigatorId))]
    public static partial SecondPlayOptionDto CardDetailToSecondOption(this CardDetail detail);
    
}