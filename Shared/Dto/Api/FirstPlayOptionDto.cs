using Domain.Enums;

namespace Shared.Dto.Api;

public class FirstPlayOptionDto
{
    public long CardId { get; set; }

    public long AvatarId { get; set; }

    public int TitleId { get; set; }

    public ShowFastSlowOption ShowFastSlowOption { get; set; }

    public ShowFeverTranceOption ShowFeverTranceOption { get; set; }
}