using GCLocalServerRewrite.models;
using SharedProject.models;

namespace GCLocalServerRewrite.common;

public static class Converters
{
    public static OnlineMatchingData ConvertFromEntry(OnlineMatchingEntry entry)
    {
        return new OnlineMatchingData
        {
            AvatarId = entry.AvatarId,
            CardId = entry.CardId,
            ClassId = entry.ClassId,
            EntryNo = entry.EntryNo,
            EntryStart = entry.EntryStart,
            EventId = entry.EventId,
            GroupId = entry.GroupId,
            MachineId = entry.MachineId,
            MatchingId = entry.MatchingId,
            MatchingRemainingTime = entry.MatchingRemainingTime,
            Pref = entry.Pref,
            Status = entry.Status,
            MatchingTimeout = entry.MatchingTimeout,
            MessageId = entry.MessageId,
            PlayerName = entry.PlayerName,
            PrefId = entry.PrefId,
            TenpoId = entry.TenpoId,
            TenpoName = entry.TenpoName,
            TitleId = entry.TitleId,
            MatchingWaitTime = entry.MatchingWaitTime
        };
    }
    
    public static OnlineMatchingEntry ConvertFromData(OnlineMatchingData entry)
    {
        return new OnlineMatchingEntry
        {
            AvatarId = entry.AvatarId,
            CardId = entry.CardId,
            ClassId = entry.ClassId,
            EntryNo = entry.EntryNo,
            EntryStart = entry.EntryStart,
            EventId = entry.EventId,
            GroupId = entry.GroupId,
            MachineId = entry.MachineId,
            MatchingId = entry.MatchingId,
            MatchingRemainingTime = entry.MatchingRemainingTime,
            Pref = entry.Pref,
            Status = entry.Status,
            MatchingTimeout = entry.MatchingTimeout,
            MessageId = entry.MessageId,
            PlayerName = entry.PlayerName,
            PrefId = entry.PrefId,
            TenpoId = entry.TenpoId,
            TenpoName = entry.TenpoName,
            TitleId = entry.TitleId,
            MatchingWaitTime = entry.MatchingWaitTime
        };
    }
}