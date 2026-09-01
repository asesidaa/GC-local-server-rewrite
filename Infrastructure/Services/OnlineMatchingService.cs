using Application.Game.Card.OnlineMatching;
using Application.Interfaces;
using Domain.Config;
using Domain.Enums;
using Domain.Models.OnlineMatching;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public sealed class OnlineMatchingService : IOnlineMatchingService
{
    private const int MaxIdAllocationAttempts = 4096;

    private readonly Lock gate = new();
    private readonly Dictionary<uint, OnlineMatchRoom> rooms = [];
    private readonly Dictionary<long, uint> roomByCard = [];
    private readonly Dictionary<uint, uint> roomByMachine = [];
    private readonly Dictionary<uint, DateTimeOffset> matchingIdTombstones = [];
    private readonly TimeProvider timeProvider;
    private readonly OnlineMatchingConfig options;
    private readonly IMatchingIdGenerator matchingIdGenerator;
    private readonly ILogger<OnlineMatchingService> logger;

    public OnlineMatchingService(
        TimeProvider timeProvider,
        IOptions<OnlineMatchingConfig> options,
        IMatchingIdGenerator matchingIdGenerator,
        ILogger<OnlineMatchingService> logger)
    {
        this.timeProvider = timeProvider;
        this.options = options.Value;
        this.matchingIdGenerator = matchingIdGenerator;
        this.logger = logger;
    }

    public OnlineMatchingServiceResult<OnlineMatchRoomSnapshot> Enter(OnlineMatchParticipant participant)
    {
        ArgumentNullException.ThrowIfNull(participant);
        var now = timeProvider.GetUtcNow();

        lock (gate)
        {
            ExpireTombstones(now);
            AdvanceRooms(now);

            if (participant.CardId <= 0 || participant.MachineId == 0 || participant.EventId == 0)
            {
                return OnlineMatchingServiceResult<OnlineMatchRoomSnapshot>.Failure(
                    OnlineMatchingServiceError.InvalidIdentity,
                    "Card, machine, and event identifiers must be nonzero.");
            }

            if (roomByCard.TryGetValue(participant.CardId, out var existingCardRoomId))
            {
                if (!rooms.TryGetValue(existingCardRoomId, out var existingRoom))
                {
                    roomByCard.Remove(participant.CardId);
                }
                else
                {
                    var outcome = existingRoom.RefreshParticipant(participant, now);
                    if (outcome == OnlineMatchJoinOutcome.AlreadyJoined)
                    {
                        return OnlineMatchingServiceResult<OnlineMatchRoomSnapshot>.Success(
                            existingRoom.CreateSnapshot(now));
                    }

                    return IdentityConflict(participant);
                }
            }

            if (roomByMachine.TryGetValue(participant.MachineId, out var existingMachineRoomId) &&
                rooms.ContainsKey(existingMachineRoomId))
            {
                return IdentityConflict(participant);
            }

            foreach (var room in rooms.Values
                         .Where(candidate => candidate.EventId == participant.EventId && candidate.CanAccept(now))
                         .OrderBy(candidate => candidate.CreatedAt)
                         .ThenBy(candidate => candidate.MatchingId))
            {
                var outcome = room.TryJoin(participant, now);
                if (outcome != OnlineMatchJoinOutcome.Joined)
                {
                    continue;
                }

                IndexParticipant(room.MatchingId, participant);
                var snapshot = room.CreateSnapshot(now);
                logger.LogInformation(
                    "Card {CardId} joined online match {MatchingId} as entry {EntryNumber} for event {EventId}",
                    participant.CardId,
                    room.MatchingId,
                    snapshot.Participants.Single(entry => entry.CardId == participant.CardId).EntryNumber,
                    participant.EventId);
                return OnlineMatchingServiceResult<OnlineMatchRoomSnapshot>.Success(snapshot);
            }

            var matchingId = AllocateMatchingId();
            var newRoom = new OnlineMatchRoom(matchingId, participant, now, options);
            rooms.Add(matchingId, newRoom);
            IndexParticipant(matchingId, participant);
            logger.LogInformation(
                "Card {CardId} created online match {MatchingId} for event {EventId}",
                participant.CardId,
                matchingId,
                participant.EventId);
            return OnlineMatchingServiceResult<OnlineMatchRoomSnapshot>.Success(newRoom.CreateSnapshot(now));
        }
    }

    public OnlineMatchingServiceResult<OnlineMatchRoomSnapshot> Update(
        uint matchingId,
        uint eventId,
        long cardId,
        uint action,
        uint messageId)
    {
        var now = timeProvider.GetUtcNow();

        lock (gate)
        {
            if (!rooms.TryGetValue(matchingId, out var room))
            {
                return OnlineMatchingServiceResult<OnlineMatchRoomSnapshot>.Failure(
                    OnlineMatchingServiceError.RoomNotFound,
                    $"Online match {matchingId} does not exist.");
            }

            room.Advance(now);
            if (room.EventId != eventId)
            {
                return OnlineMatchingServiceResult<OnlineMatchRoomSnapshot>.Failure(
                    OnlineMatchingServiceError.EventMismatch,
                    $"Event {eventId} does not own online match {matchingId}.");
            }

            var participant = room.FindParticipant(cardId, now);
            if (participant is null)
            {
                return OnlineMatchingServiceResult<OnlineMatchRoomSnapshot>.Failure(
                    OnlineMatchingServiceError.NotMember,
                    $"Card {cardId} does not belong to online match {matchingId}.");
            }

            if (action != 0)
            {
                return OnlineMatchingServiceResult<OnlineMatchRoomSnapshot>.Failure(
                    OnlineMatchingServiceError.InvalidAction,
                    $"Online matching update action must be the native value 0, received {action}.");
            }

            room.TryUpdateMessage(cardId, messageId, now);
            return OnlineMatchingServiceResult<OnlineMatchRoomSnapshot>.Success(room.CreateSnapshot(now));
        }
    }

    public OnlineMatchingServiceResult<bool> SubmitResult(long cardId, OnlineMatchResultSubmission submission)
    {
        ArgumentNullException.ThrowIfNull(submission);
        var now = timeProvider.GetUtcNow();

        lock (gate)
        {
            if (!rooms.TryGetValue(submission.MatchingId, out var room))
            {
                return OnlineMatchingServiceResult<bool>.Failure(
                    OnlineMatchingServiceError.RoomNotFound,
                    $"Online match {submission.MatchingId} does not exist.");
            }

            if (room.EventId != submission.EventId)
            {
                return OnlineMatchingServiceResult<bool>.Failure(
                    OnlineMatchingServiceError.EventMismatch,
                    $"Event {submission.EventId} does not own online match {submission.MatchingId}.");
            }

            var wasClosed = room.Phase == OnlineMatchPhase.Closed;
            var outcome = room.TrySubmitResult(cardId, submission, now);
            switch (outcome)
            {
                case OnlineMatchResultOutcome.Accepted:
                case OnlineMatchResultOutcome.Duplicate:
                    if (!wasClosed && room.Phase == OnlineMatchPhase.Closed)
                    {
                        ReleaseIdentityIndexes(room, now);
                    }

                    return OnlineMatchingServiceResult<bool>.Success(true);

                case OnlineMatchResultOutcome.NotMember:
                    return OnlineMatchingServiceResult<bool>.Failure(
                        OnlineMatchingServiceError.NotMember,
                        $"Card {cardId} does not belong to online match {submission.MatchingId}.");

                case OnlineMatchResultOutcome.InvalidPhase:
                    return OnlineMatchingServiceResult<bool>.Failure(
                        OnlineMatchingServiceError.InvalidPhase,
                        $"Online match {submission.MatchingId} is not ready for results.");

                case OnlineMatchResultOutcome.ConflictingResult:
                    return OnlineMatchingServiceResult<bool>.Failure(
                        OnlineMatchingServiceError.ConflictingResult,
                        $"Card {cardId} submitted a conflicting result for online match {submission.MatchingId}.");

                default:
                    throw new InvalidOperationException($"Unknown result outcome {outcome}.");
            }
        }
    }

    public void CleanupStaleRooms()
    {
        var now = timeProvider.GetUtcNow();
        var removed = 0;

        lock (gate)
        {
            ExpireTombstones(now);

            foreach (var room in rooms.Values.ToArray())
            {
                room.Advance(now);
                var shouldRemove = room.Phase == OnlineMatchPhase.Closed
                    ? room.ClosedAt is { } closedAt &&
                      now - closedAt >= TimeSpan.FromSeconds(options.TerminalRetentionSeconds)
                    : now - room.LastActivityAt >= TimeSpan.FromSeconds(options.RoomIdleExpirySeconds);
                if (!shouldRemove)
                {
                    continue;
                }

                RemoveRoom(room, now);
                removed++;
            }
        }

        if (removed > 0)
        {
            logger.LogInformation("Cleaned up {Count} expired online match rooms", removed);
        }
    }

    private uint AllocateMatchingId()
    {
        for (var attempt = 0; attempt < MaxIdAllocationAttempts; attempt++)
        {
            var candidate = matchingIdGenerator.Next();
            if (candidate != 0 && !rooms.ContainsKey(candidate) && !matchingIdTombstones.ContainsKey(candidate))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException("Unable to allocate a unique online matching ID.");
    }

    private void AdvanceRooms(DateTimeOffset now)
    {
        foreach (var room in rooms.Values)
        {
            room.Advance(now);
        }
    }

    private void IndexParticipant(uint matchingId, OnlineMatchParticipant participant)
    {
        roomByCard[participant.CardId] = matchingId;
        roomByMachine[participant.MachineId] = matchingId;
    }

    private void ReleaseIdentityIndexes(OnlineMatchRoom room, DateTimeOffset now)
    {
        foreach (var participant in room.CreateSnapshot(now).Participants)
        {
            if (roomByCard.TryGetValue(participant.CardId, out var cardRoomId) && cardRoomId == room.MatchingId)
            {
                roomByCard.Remove(participant.CardId);
            }

            if (roomByMachine.TryGetValue(participant.MachineId, out var machineRoomId) &&
                machineRoomId == room.MatchingId)
            {
                roomByMachine.Remove(participant.MachineId);
            }
        }
    }

    private void RemoveRoom(OnlineMatchRoom room, DateTimeOffset now)
    {
        rooms.Remove(room.MatchingId);
        ReleaseIdentityIndexes(room, now);
        matchingIdTombstones[room.MatchingId] = now.AddSeconds(options.MatchingIdReuseGuardSeconds);
    }

    private void ExpireTombstones(DateTimeOffset now)
    {
        foreach (var (matchingId, expiresAt) in matchingIdTombstones.ToArray())
        {
            if (now >= expiresAt)
            {
                matchingIdTombstones.Remove(matchingId);
            }
        }
    }

    private static OnlineMatchingServiceResult<OnlineMatchRoomSnapshot> IdentityConflict(
        OnlineMatchParticipant participant)
    {
        return OnlineMatchingServiceResult<OnlineMatchRoomSnapshot>.Failure(
            OnlineMatchingServiceError.IdentityConflict,
            $"Card {participant.CardId} or machine {participant.MachineId} is already matching with another identity.");
    }

}
