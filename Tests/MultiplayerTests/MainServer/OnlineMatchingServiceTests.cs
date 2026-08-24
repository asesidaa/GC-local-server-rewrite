using Application.Game.Card.OnlineMatching;
using Application.Interfaces;
using Domain.Config;
using Domain.Enums;
using Domain.Models.OnlineMatching;
using Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace MultiplayerTests.MainServer;

public class OnlineMatchingServiceTests
{
    private static readonly DateTimeOffset Start = new(2026, 8, 25, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void EnterRetry_ReturnsSameMatchingIdAndEntry()
    {
        var fixture = CreateFixture([101]);
        var first = fixture.Service.Enter(CreateParticipant(1));
        var retryProfile = CreateParticipant(1) with
        {
            MessageId = 999,
            EntryStart = Start.AddSeconds(5)
        };

        var retry = fixture.Service.Enter(retryProfile);

        Assert.True(first.IsSuccess);
        Assert.True(retry.IsSuccess);
        Assert.Equal(101u, retry.Value!.MatchingId);
        var participant = Assert.Single(retry.Value.Participants);
        Assert.Equal(0u, participant.EntryNumber);
        Assert.Equal(999u, participant.MessageId);
    }

    [Fact]
    public void DifferentEvents_CreateDifferentRooms()
    {
        var fixture = CreateFixture([101, 202]);

        var first = fixture.Service.Enter(CreateParticipant(1, eventId: 7));
        var second = fixture.Service.Enter(CreateParticipant(2, eventId: 8));

        Assert.Equal(101u, first.Value!.MatchingId);
        Assert.Equal(202u, second.Value!.MatchingId);
        Assert.Equal(0u, first.Value.Participants.Single().EntryNumber);
        Assert.Equal(0u, second.Value.Participants.Single().EntryNumber);
    }

    [Fact]
    public void ConflictingCardOrMachine_IsRejected()
    {
        var fixture = CreateFixture([101]);
        fixture.Service.Enter(CreateParticipant(1, machineId: 1001));

        var cardConflict = fixture.Service.Enter(CreateParticipant(1, machineId: 2002));
        var machineConflict = fixture.Service.Enter(CreateParticipant(2, machineId: 1001));

        Assert.Equal(OnlineMatchingServiceError.IdentityConflict, cardConflict.Error);
        Assert.Equal(OnlineMatchingServiceError.IdentityConflict, machineConflict.Error);
    }

    [Fact]
    public void EntriesRemainStableAndFifthParticipantStartsAnotherRoom()
    {
        var fixture = CreateFixture([101, 202]);
        var admissions = Enumerable.Range(1, 5)
            .Select(cardId => fixture.Service.Enter(CreateParticipant(cardId)))
            .ToArray();

        Assert.All(admissions, result => Assert.True(result.IsSuccess));
        Assert.All(admissions[..4], result => Assert.Equal(101u, result.Value!.MatchingId));
        Assert.Equal([0u, 1u, 2u, 3u], admissions[..4].Select(result => result.Value!.Participants.Last().EntryNumber));
        Assert.Equal(202u, admissions[4].Value!.MatchingId);
        Assert.Equal(0u, admissions[4].Value!.Participants.Single().EntryNumber);
    }

    [Fact]
    public void Update_ValidatesMembershipEventAndEntryDependentAction()
    {
        var fixture = CreateFixture([101]);
        fixture.Service.Enter(CreateParticipant(1));
        fixture.Service.Enter(CreateParticipant(2));

        var hostWrongAction = fixture.Service.Update(101, 7, 1, 2, 500);
        var guestWrongAction = fixture.Service.Update(101, 7, 2, 1, 500);
        var wrongEvent = fixture.Service.Update(101, 8, 1, 1, 500);
        var unknownCard = fixture.Service.Update(101, 7, 99, 2, 500);
        var valid = fixture.Service.Update(101, 7, 2, 2, 777);

        Assert.Equal(OnlineMatchingServiceError.InvalidAction, hostWrongAction.Error);
        Assert.Equal(OnlineMatchingServiceError.InvalidAction, guestWrongAction.Error);
        Assert.Equal(OnlineMatchingServiceError.EventMismatch, wrongEvent.Error);
        Assert.Equal(OnlineMatchingServiceError.NotMember, unknownCard.Error);
        Assert.True(valid.IsSuccess);
        Assert.Equal(70u, valid.Value!.Participants.Single(p => p.CardId == 1).MessageId);
        Assert.Equal(777u, valid.Value.Participants.Single(p => p.CardId == 2).MessageId);
    }

    [Fact]
    public void UnevenPolling_DoesNotAccelerateCountdown()
    {
        var fixture = CreateFixture([101]);
        fixture.Service.Enter(CreateParticipant(1));
        fixture.Clock.Advance(TimeSpan.FromSeconds(5));

        var repeated = Enumerable.Range(0, 20)
            .Select(index => fixture.Service.Update(101, 7, 1, 1, checked((uint)index)))
            .ToArray();

        Assert.All(repeated, result => Assert.Equal(new OnlineMatchTiming(94, 10, 84), result.Value!.Timing));
        fixture.Clock.Advance(TimeSpan.FromSeconds(1));
        var oneSecondLater = fixture.Service.Update(101, 7, 1, 1, 21);
        Assert.Equal(new OnlineMatchTiming(93, 10, 83), oneSecondLater.Value!.Timing);
    }

    [Fact]
    public void Cleanup_UsesLastActivityRatherThanCreationTime()
    {
        var options = CreateOptions() with { RoomIdleExpirySeconds = 10 };
        var fixture = CreateFixture([101], options);
        fixture.Service.Enter(CreateParticipant(1));
        fixture.Clock.Advance(TimeSpan.FromSeconds(9));
        fixture.Service.Update(101, 7, 1, 1, 80);
        fixture.Clock.Advance(TimeSpan.FromSeconds(9));

        fixture.Service.CleanupStaleRooms();
        var stillPresent = fixture.Service.Update(101, 7, 1, 1, 81);
        fixture.Clock.Advance(TimeSpan.FromSeconds(11));
        fixture.Service.CleanupStaleRooms();
        var removed = fixture.Service.Update(101, 7, 1, 1, 82);

        Assert.True(stillPresent.IsSuccess);
        Assert.Equal(OnlineMatchingServiceError.RoomNotFound, removed.Error);
    }

    [Fact]
    public void RemovedMatchingId_IsGuardedAgainstImmediateReuse()
    {
        var options = CreateOptions() with
        {
            RoomIdleExpirySeconds = 1,
            MatchingIdReuseGuardSeconds = 100
        };
        var fixture = CreateFixture([101, 101, 202], options);
        fixture.Service.Enter(CreateParticipant(1));
        fixture.Clock.Advance(TimeSpan.FromSeconds(2));
        fixture.Service.CleanupStaleRooms();

        var next = fixture.Service.Enter(CreateParticipant(2));

        Assert.Equal(202u, next.Value!.MatchingId);
    }

    [Fact]
    public async Task ConcurrentAdmissions_DoNotOverfillOrDuplicateEntries()
    {
        var ids = Enumerable.Range(1, 20).Select(value => checked((uint)(value * 100))).ToArray();
        var fixture = CreateFixture(ids);

        var results = await Task.WhenAll(Enumerable.Range(1, 16)
            .Select(cardId => Task.Run(() => fixture.Service.Enter(CreateParticipant(cardId)))));

        Assert.All(results, result => Assert.True(result.IsSuccess));
        var groups = results.GroupBy(result => result.Value!.MatchingId).ToArray();
        Assert.Equal(4, groups.Length);
        Assert.All(groups, group =>
        {
            var final = group.OrderByDescending(result => result.Value!.Participants.Count).First().Value!;
            Assert.Equal(4, final.Participants.Count);
            Assert.Equal([0u, 1u, 2u, 3u], final.Participants.Select(p => p.EntryNumber));
        });
    }

    [Fact]
    public void ResultSubmission_IsIdempotentAndClosesAfterEveryParticipant()
    {
        var options = CreateOptions() with { MaxPlayers = 2 };
        var fixture = CreateFixture([101, 202], options);
        fixture.Service.Enter(CreateParticipant(1));
        fixture.Service.Enter(CreateParticipant(2));
        var firstResult = CreateResult(101, 1);

        var first = fixture.Service.SubmitResult(1, firstResult);
        var retry = fixture.Service.SubmitResult(1, firstResult);
        var conflict = fixture.Service.SubmitResult(1, firstResult with { ResultScore = 999999 });
        var second = fixture.Service.SubmitResult(2, CreateResult(101, 2));
        var nextAdmission = fixture.Service.Enter(CreateParticipant(1));

        Assert.True(first.IsSuccess);
        Assert.True(retry.IsSuccess);
        Assert.Equal(OnlineMatchingServiceError.ConflictingResult, conflict.Error);
        Assert.True(second.IsSuccess);
        Assert.Equal(202u, nextAdmission.Value!.MatchingId);
    }

    private static Fixture CreateFixture(uint[] ids, OnlineMatchingConfig? options = null)
    {
        var clock = new ManualTimeProvider(Start);
        var service = new OnlineMatchingService(
            clock,
            Options.Create(options ?? CreateOptions()),
            new SequenceMatchingIdGenerator(ids),
            NullLogger<OnlineMatchingService>.Instance);
        return new Fixture(service, clock);
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

    private static OnlineMatchParticipant CreateParticipant(
        long cardId,
        uint eventId = 7,
        uint? machineId = null)
    {
        return new OnlineMatchParticipant(
            cardId,
            machineId ?? checked((uint)(1000 + cardId)),
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

    private static OnlineMatchResultSubmission CreateResult(uint matchingId, uint seed)
    {
        return new OnlineMatchResultSubmission(
            7,
            matchingId,
            30,
            40,
            100000 + seed,
            100 + seed,
            seed,
            1000 + seed,
            2000 + seed,
            3000 + seed,
            0,
            1,
            2,
            10 + seed,
            20 + seed,
            30 + seed);
    }

    private sealed record Fixture(OnlineMatchingService Service, ManualTimeProvider Clock);

    private sealed class SequenceMatchingIdGenerator(IEnumerable<uint> ids) : IMatchingIdGenerator
    {
        private readonly Queue<uint> ids = new(ids);

        public uint Next()
        {
            lock (ids)
            {
                return ids.Dequeue();
            }
        }
    }
}
