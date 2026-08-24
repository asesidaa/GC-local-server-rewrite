namespace Domain.Models.OnlineMatching;

public sealed record OnlineMatchParticipant(
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
    DateTimeOffset EntryStart);
