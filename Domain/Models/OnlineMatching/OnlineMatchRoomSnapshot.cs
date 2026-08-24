using Domain.Enums;

namespace Domain.Models.OnlineMatching;

public sealed record OnlineMatchTiming(uint Timeout, uint Wait, uint Remaining);

public sealed record OnlineMatchParticipantSnapshot(
    uint EntryNumber,
    long CardId,
    uint MachineId,
    uint EventId,
    string PlayerName,
    uint AvatarId,
    uint TitleId,
    uint ClassId,
    uint GroupId,
    uint TenpoId,
    string TenpoName,
    uint PrefId,
    string Pref,
    uint MessageId,
    DateTimeOffset EntryStart,
    bool HasResult);

public sealed record OnlineMatchRoomSnapshot(
    uint MatchingId,
    uint EventId,
    OnlineMatchPhase Phase,
    OnlineMatchTiming Timing,
    IReadOnlyList<OnlineMatchParticipantSnapshot> Participants);
