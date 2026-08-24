using Domain.Config;
using Domain.Enums;
using Domain.Models.OnlineMatching;

namespace MultiplayerTests.MainServer;

public class OnlineMatchRoomTests
{
    private static readonly DateTimeOffset Start = new(2026, 8, 25, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void FirstParticipant_UsesSearchDeadlineAnd99_10_89Timing()
    {
        var room = CreateRoom();

        var snapshot = room.CreateSnapshot(Start);

        Assert.Equal(OnlineMatchPhase.Searching, snapshot.Phase);
        Assert.Equal(new OnlineMatchTiming(99, 10, 89), snapshot.Timing);
        var participant = Assert.Single(snapshot.Participants);
        Assert.Equal(0u, participant.EntryNumber);
        Assert.Equal(1L, participant.CardId);
    }

    [Fact]
    public void SecondParticipant_StartsOneFortySecondFormationWindow()
    {
        var room = CreateRoom();
        var joinedAt = Start.AddSeconds(20);

        var outcome = room.TryJoin(CreateParticipant(2), joinedAt);
        var snapshot = room.CreateSnapshot(joinedAt);

        Assert.Equal(OnlineMatchJoinOutcome.Joined, outcome);
        Assert.Equal(OnlineMatchPhase.Forming, snapshot.Phase);
        Assert.Equal(new OnlineMatchTiming(40, 10, 30), snapshot.Timing);
        Assert.Equal([0u, 1u], snapshot.Participants.Select(p => p.EntryNumber));
    }

    [Fact]
    public void ThirdParticipant_DoesNotRestartFormationDeadline()
    {
        var room = CreateRoom();
        room.TryJoin(CreateParticipant(2), Start.AddSeconds(20));

        var outcome = room.TryJoin(CreateParticipant(3), Start.AddSeconds(30));
        var snapshot = room.CreateSnapshot(Start.AddSeconds(30));

        Assert.Equal(OnlineMatchJoinOutcome.Joined, outcome);
        Assert.Equal(OnlineMatchPhase.Forming, snapshot.Phase);
        Assert.Equal(new OnlineMatchTiming(30, 10, 20), snapshot.Timing);
    }

    [Fact]
    public void FourthParticipant_BecomesReadyImmediately()
    {
        var room = CreateRoom();
        room.TryJoin(CreateParticipant(2), Start.AddSeconds(1));
        room.TryJoin(CreateParticipant(3), Start.AddSeconds(2));

        var outcome = room.TryJoin(CreateParticipant(4), Start.AddSeconds(3));
        var snapshot = room.CreateSnapshot(Start.AddSeconds(3));

        Assert.Equal(OnlineMatchJoinOutcome.Joined, outcome);
        Assert.Equal(OnlineMatchPhase.Ready, snapshot.Phase);
        Assert.Equal(new OnlineMatchTiming(0, 0, 0), snapshot.Timing);
        Assert.Equal(4, snapshot.Participants.Count);
    }

    [Fact]
    public void SearchDeadline_WithOneParticipant_BecomesReady()
    {
        var room = CreateRoom();

        var snapshot = room.CreateSnapshot(Start.AddSeconds(99));

        Assert.Equal(OnlineMatchPhase.Ready, snapshot.Phase);
        Assert.Equal(new OnlineMatchTiming(0, 0, 0), snapshot.Timing);
    }

    [Fact]
    public void FormationDeadline_WithThreeParticipants_BecomesReady()
    {
        var room = CreateRoom();
        room.TryJoin(CreateParticipant(2), Start.AddSeconds(20));
        room.TryJoin(CreateParticipant(3), Start.AddSeconds(30));

        var snapshot = room.CreateSnapshot(Start.AddSeconds(60));

        Assert.Equal(OnlineMatchPhase.Ready, snapshot.Phase);
        Assert.Equal(new OnlineMatchTiming(0, 0, 0), snapshot.Timing);
    }

    [Fact]
    public void SnapshotTiming_UsesCeilingWithoutPollCountMutation()
    {
        var room = CreateRoom();

        var first = room.CreateSnapshot(Start.AddMilliseconds(100));
        var second = room.CreateSnapshot(Start.AddMilliseconds(100));

        Assert.Equal(new OnlineMatchTiming(99, 10, 89), first.Timing);
        Assert.Equal(first.Phase, second.Phase);
        Assert.Equal(first.Timing, second.Timing);
        Assert.Equal(first.Participants.Select(p => p.EntryNumber), second.Participants.Select(p => p.EntryNumber));
    }

    private static OnlineMatchRoom CreateRoom()
    {
        return new OnlineMatchRoom(0x10203040, CreateParticipant(1), Start, CreateOptions());
    }

    private static OnlineMatchingConfig CreateOptions()
    {
        return new OnlineMatchingConfig
        {
            SearchTimeoutSeconds = 99,
            FormationTimeoutSeconds = 40,
            PollIntervalSeconds = 10,
            RoomIdleExpirySeconds = 600,
            TerminalRetentionSeconds = 120,
            MatchingIdReuseGuardSeconds = 600,
            MaxPlayers = 4
        };
    }

    private static OnlineMatchParticipant CreateParticipant(long cardId, uint eventId = 7)
    {
        return new OnlineMatchParticipant(
            cardId,
            checked((uint)(1000 + cardId)),
            eventId,
            $"Player {cardId}",
            10,
            20,
            30,
            40,
            50,
            "Test Shop",
            60,
            "Test Prefecture",
            70,
            Start);
    }
}
