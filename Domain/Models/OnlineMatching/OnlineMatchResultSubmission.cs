namespace Domain.Models.OnlineMatching;

public sealed record OnlineMatchResultSubmission(
    uint EventId,
    uint MatchingId,
    uint ClassId,
    uint GroupId,
    uint ResultScore,
    uint ResultStar,
    uint ResultRank,
    uint MusicId1,
    uint MusicId2,
    uint MusicId3,
    uint DifficultyLevel1,
    uint DifficultyLevel2,
    uint DifficultyLevel3,
    uint ItemId1,
    uint ItemId2,
    uint ItemId3);
