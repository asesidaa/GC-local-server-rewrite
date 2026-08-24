using Domain.Config;
using Domain.Enums;

namespace Domain.Models.OnlineMatching;

public sealed class OnlineMatchRoom
{
    private readonly OnlineMatchingConfig options;
    private readonly List<ParticipantState> participants = [];
    private DateTimeOffset phaseDeadline;

    public OnlineMatchRoom(
        uint matchingId,
        OnlineMatchParticipant firstParticipant,
        DateTimeOffset now,
        OnlineMatchingConfig options)
    {
        ArgumentNullException.ThrowIfNull(firstParticipant);
        ArgumentNullException.ThrowIfNull(options);

        if (matchingId == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(matchingId));
        }

        if (firstParticipant.MachineId == 0 || firstParticipant.EventId == 0)
        {
            throw new ArgumentException("Machine and event identifiers must be nonzero.", nameof(firstParticipant));
        }

        if (options.MaxPlayers is < 1 or > 4)
        {
            throw new ArgumentOutOfRangeException(nameof(options.MaxPlayers));
        }

        MatchingId = matchingId;
        EventId = firstParticipant.EventId;
        CreatedAt = now;
        LastActivityAt = now;
        Phase = OnlineMatchPhase.Searching;
        phaseDeadline = now.AddSeconds(options.SearchTimeoutSeconds);
        this.options = options;
        participants.Add(new ParticipantState(0, firstParticipant));

        if (options.MaxPlayers == 1)
        {
            BecomeReady(now);
        }
    }

    public uint MatchingId { get; }

    public uint EventId { get; }

    public OnlineMatchPhase Phase { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset LastActivityAt { get; private set; }

    public DateTimeOffset? ReadyAt { get; private set; }

    public DateTimeOffset? ClosedAt { get; private set; }

    public int ParticipantCount => participants.Count;

    public OnlineMatchJoinOutcome TryJoin(OnlineMatchParticipant participant, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(participant);
        Advance(now);

        var sameCard = participants.FirstOrDefault(state => state.Profile.CardId == participant.CardId);
        if (sameCard is not null)
        {
            return sameCard.Profile.MachineId == participant.MachineId && participant.EventId == EventId
                ? OnlineMatchJoinOutcome.AlreadyJoined
                : OnlineMatchJoinOutcome.CardConflict;
        }

        if (participants.Any(state => state.Profile.MachineId == participant.MachineId))
        {
            return OnlineMatchJoinOutcome.MachineConflict;
        }

        if (participant.EventId != EventId)
        {
            return OnlineMatchJoinOutcome.EventMismatch;
        }

        if (Phase is not (OnlineMatchPhase.Searching or OnlineMatchPhase.Forming))
        {
            return OnlineMatchJoinOutcome.NotJoinable;
        }

        if (participants.Count >= options.MaxPlayers)
        {
            return OnlineMatchJoinOutcome.Full;
        }

        var entryNumber = Enumerable.Range(0, options.MaxPlayers)
            .Select(value => (uint)value)
            .First(value => participants.All(state => state.EntryNumber != value));
        participants.Add(new ParticipantState(entryNumber, participant));
        LastActivityAt = now;

        if (participants.Count == 2 && Phase == OnlineMatchPhase.Searching)
        {
            Phase = OnlineMatchPhase.Forming;
            phaseDeadline = now.AddSeconds(options.FormationTimeoutSeconds);
        }

        if (participants.Count == options.MaxPlayers)
        {
            BecomeReady(now);
        }

        return OnlineMatchJoinOutcome.Joined;
    }

    public void Advance(DateTimeOffset now)
    {
        if (Phase is OnlineMatchPhase.Searching or OnlineMatchPhase.Forming && now >= phaseDeadline)
        {
            BecomeReady(now);
        }
    }

    public OnlineMatchRoomSnapshot CreateSnapshot(DateTimeOffset now)
    {
        Advance(now);

        var timing = CreateTiming(now);
        var participantSnapshots = participants
            .OrderBy(state => state.EntryNumber)
            .Select(state => new OnlineMatchParticipantSnapshot(
                state.EntryNumber,
                state.Profile.CardId,
                state.Profile.MachineId,
                state.Profile.EventId,
                state.Profile.PlayerName,
                state.Profile.AvatarId,
                state.Profile.TitleId,
                state.Profile.ClassId,
                state.Profile.GroupId,
                state.Profile.TenpoId,
                state.Profile.TenpoName,
                state.Profile.PrefId,
                state.Profile.Pref,
                state.Profile.MessageId,
                state.Profile.EntryStart,
                state.HasResult))
            .ToArray();

        return new OnlineMatchRoomSnapshot(MatchingId, EventId, Phase, timing, participantSnapshots);
    }

    private OnlineMatchTiming CreateTiming(DateTimeOffset now)
    {
        if (Phase is not (OnlineMatchPhase.Searching or OnlineMatchPhase.Forming))
        {
            return new OnlineMatchTiming(0, 0, 0);
        }

        var seconds = Math.Max(0, Math.Ceiling((phaseDeadline - now).TotalSeconds));
        var timeout = checked((uint)seconds);
        var wait = Math.Min(checked((uint)options.PollIntervalSeconds), timeout);
        return new OnlineMatchTiming(timeout, wait, timeout - wait);
    }

    private void BecomeReady(DateTimeOffset now)
    {
        Phase = OnlineMatchPhase.Ready;
        ReadyAt = now;
        LastActivityAt = now;
    }

    private sealed class ParticipantState(uint entryNumber, OnlineMatchParticipant profile)
    {
        public uint EntryNumber { get; } = entryNumber;

        public OnlineMatchParticipant Profile { get; set; } = profile;

        public bool HasResult { get; set; }
    }
}
