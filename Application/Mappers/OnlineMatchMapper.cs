using Application.Game.Card.OnlineMatching;
using Riok.Mapperly.Abstractions;

namespace Application.Mappers;

[Mapper]
public static partial class OnlineMatchMapper
{
    public static partial OnlineMatchEntryDto MatchEntryToDto(this MatchEntry entry);

    public static partial MatchEntry DtoToMatchEntry(this OnlineMatchEntryDto entryDto);

    private static string DateTimeToString(DateTime dateTime)
    {
        return dateTime.ToString("yyyy/MM/dd HH:mm:ss");
    }
}
