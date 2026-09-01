using System.Globalization;
using Domain.Enums;
using Domain.Models.OnlineMatching;

namespace Application.Game.Card.OnlineMatching;

public sealed class OnlineMatchingProtocolException(string fieldName, string message)
    : Exception($"Invalid online matching field '{fieldName}': {message}")
{
    public string FieldName { get; } = fieldName;
}

public sealed record OnlineMatchingUpdateValues(
    uint Action,
    uint EventId,
    uint MatchingId,
    uint MessageId);

public static class OnlineMatchingXml
{
    private const string RecordXPath = "/root/online_matching/record";

    public static OnlineMatchParticipant ToParticipant(
        OnlineMatchingEntryRequestDto dto,
        long cardId,
        DateTimeOffset entryStart)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (cardId <= 0)
        {
            throw new OnlineMatchingProtocolException("card_id", "must be positive");
        }

        return new OnlineMatchParticipant(
            cardId,
            ToUInt32(dto.MachineId, "machine_id", requireNonzero: true),
            ToUInt32(dto.EventId, "event_id", requireNonzero: true),
            dto.PlayerName,
            ToUInt32(dto.AvatarId, "avatar_id"),
            ToUInt32(dto.TitleId, "title_id"),
            ToUInt32(dto.ClassId, "class_id"),
            ToUInt32(dto.GroupId, "group_id"),
            ToUInt32(dto.TenpoId, "tenpo_id"),
            dto.TenpoName,
            ToUInt32(dto.PrefId, "pref_id"),
            dto.Pref,
            ToUInt32(dto.MessageId, "message_id"),
            entryStart);
    }

    public static OnlineMatchingUpdateValues ToUpdate(OnlineMatchingUpdateRequestDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var action = ToUInt32(dto.Action, "action");
        if (action != 0)
        {
            throw new OnlineMatchingProtocolException("action", "must be the native value 0");
        }

        return new OnlineMatchingUpdateValues(
            action,
            ToUInt32(dto.EventId, "event_id", requireNonzero: true),
            ToUInt32(dto.MatchingId, "matching_id", requireNonzero: true),
            ToUInt32(dto.MessageId, "message_id"));
    }

    public static OnlineMatchResultSubmission ToResult(OnlineMatchingResultRequestDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return new OnlineMatchResultSubmission(
            ToUInt32(dto.EventId, "event_id", requireNonzero: true),
            ToUInt32(dto.MatchingId, "matching_id", requireNonzero: true),
            ToUInt32(dto.ClassId, "class_id"),
            ToUInt32(dto.GroupId, "group_id"),
            ToUInt32(dto.ResultScore, "result_score"),
            ToUInt32(dto.ResultStar, "result_star"),
            ToUInt32(dto.ResultRank, "result_rank"),
            ToUInt32(dto.MusicId1, "music_id_1st"),
            ToUInt32(dto.MusicId2, "music_id_2nd"),
            ToUInt32(dto.MusicId3, "music_id_3rd"),
            ToUInt32(dto.DifficultyLevel1, "difficulty_lv_1st"),
            ToUInt32(dto.DifficultyLevel2, "difficulty_lv_2nd"),
            ToUInt32(dto.DifficultyLevel3, "difficulty_lv_3rd"),
            ToUInt32(dto.ItemId1, "item_id_1st"),
            ToUInt32(dto.ItemId2, "item_id_2nd"),
            ToUInt32(dto.ItemId3, "item_id_3rd"));
    }

    public static string SerializeSnapshot(OnlineMatchRoomSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        var status = snapshot.Phase is OnlineMatchPhase.Searching or OnlineMatchPhase.Forming ? 1u : 3u;
        var records = snapshot.Participants.Select((participant, index) => new OnlineMatchingRecordDto
        {
            Id = index,
            CardId = participant.CardId,
            PlayerName = participant.PlayerName,
            MachineId = participant.MachineId,
            EventId = participant.EventId,
            AvatarId = participant.AvatarId,
            TitleId = participant.TitleId,
            ClassId = participant.ClassId,
            GroupId = participant.GroupId,
            TenpoId = participant.TenpoId,
            TenpoName = participant.TenpoName,
            PrefId = participant.PrefId,
            Pref = participant.Pref,
            MessageId = participant.MessageId,
            MatchingId = snapshot.MatchingId,
            EntryNumber = participant.EntryNumber,
            Status = status,
            MatchingTimeout = snapshot.Timing.Timeout,
            MatchingWaitTime = snapshot.Timing.Wait,
            MatchingRemainingTime = snapshot.Timing.Remaining,
            EntryStart = participant.EntryStart.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture)
        });
        return records.SerializeCardDataList(RecordXPath);
    }

    private static uint ToUInt32(long value, string fieldName, bool requireNonzero = false)
    {
        if (value < 0 || value > uint.MaxValue || requireNonzero && value == 0)
        {
            var range = requireNonzero ? $"1..{uint.MaxValue}" : $"0..{uint.MaxValue}";
            throw new OnlineMatchingProtocolException(fieldName, $"must be in {range}");
        }

        return (uint)value;
    }
}
