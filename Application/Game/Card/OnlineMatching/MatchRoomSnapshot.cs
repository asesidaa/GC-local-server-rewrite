using Application.Mappers;

namespace Application.Game.Card.OnlineMatching;

public record MatchRoomSnapshot(long MatchId, bool IsOpen, IReadOnlyList<MatchEntry> Entries)
{
    private const string RECORD_XPATH = "/root/online_matching/record";

    public string SerializeEntries()
    {
        return Entries.Select((e, id) =>
        {
            var entryDto = e.MatchEntryToDto();
            entryDto.Id = id;
            return entryDto;
        }).SerializeCardDataList(RECORD_XPATH);
    }
}
