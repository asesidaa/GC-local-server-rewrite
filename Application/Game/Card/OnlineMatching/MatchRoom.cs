namespace Application.Game.Card.OnlineMatching;

public class MatchRoom
{
    private readonly Lock _lock = new();

    public long MatchId { get; }

    public bool IsOpen { get; private set; } = true;

    public DateTime CreatedAt { get; } = DateTime.UtcNow;

    private readonly List<MatchEntry> entries = [];

    public MatchRoom(long matchId)
    {
        MatchId = matchId;
    }

    /// <summary>
    /// Atomically checks capacity and adds the entry. Returns null if room is full or closed.
    /// </summary>
    public MatchRoomSnapshot? TryAddEntry(MatchEntry entry)
    {
        lock (_lock)
        {
            if (!IsOpen || entries.Count >= 4)
            {
                return null;
            }

            entry.MatchId = MatchId;
            entry.EntryId = entries.Count;
            entries.Add(entry);
            return CreateSnapshot();
        }
    }

    public MatchRoomSnapshot Update(long cardId, long messageId)
    {
        lock (_lock)
        {
            foreach (var entry in entries)
            {
                if (entry.CardId == cardId)
                {
                    entry.MessageId = messageId;
                }

                entry.MatchRemainingTime--;
            }

            if (entries.TrueForAll(e => e.MatchRemainingTime <= 0))
            {
                entries.ForEach(e => e.Status = 3);
                IsOpen = false;
            }

            return CreateSnapshot();
        }
    }

    public void Close()
    {
        lock (_lock)
        {
            IsOpen = false;
        }
    }

    private MatchRoomSnapshot CreateSnapshot()
    {
        var copy = entries.Select(e => new MatchEntry
        {
            MatchId = e.MatchId,
            EntryId = e.EntryId,
            MachineId = e.MachineId,
            EventId = e.EventId,
            StartTime = e.StartTime,
            Status = e.Status,
            CardId = e.CardId,
            PlayerName = e.PlayerName,
            AvatarId = e.AvatarId,
            TitleId = e.TitleId,
            ClassId = e.ClassId,
            GroupId = e.GroupId,
            TenpoId = e.TenpoId,
            TenpoName = e.TenpoName,
            PrefId = e.PrefId,
            Pref = e.Pref,
            MessageId = e.MessageId,
            MatchTimeout = e.MatchTimeout,
            MatchWaitTime = e.MatchWaitTime,
            MatchRemainingTime = e.MatchRemainingTime
        }).ToList();

        return new MatchRoomSnapshot(MatchId, IsOpen, copy);
    }
}
