using Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Application.Mappers;

[Mapper]
public static partial class OnlineMatchMapper
{
    public static partial OnlineMatchEntryDto OnlineMatchEntryToDto(this OnlineMatchEntry entry);

    public static partial OnlineMatchEntry DtoToOnlineMatchEntry(this OnlineMatchEntryDto entryDto);
    
    private static string DateTimeToString(DateTime dateTime)
    {
        return dateTime.ToString("yyyy/MM/dd hh:mm:ss");
    }
}