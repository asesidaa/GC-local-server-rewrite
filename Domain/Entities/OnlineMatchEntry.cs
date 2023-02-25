namespace Domain.Entities;

public partial class OnlineMatchEntry
{
    public long MatchId { get; set; }
    
    public long EntryId { get; set; }
    
    public long MachineId { get; set; }

    public long EventId { get; set; }
    
    public DateTime StartTime { get; set; }

    public long Status { get; set; }

    public long CardId { get; set; }

    public string PlayerName { get; set; } = string.Empty;

    public long AvatarId { get; set; }

    public long TitleId { get; set; }

    public long ClassId { get; set; }

    public long GroupId { get; set; }

    public long TenpoId { get; set; } = 1337;

    public string TenpoName { get; set; } = "GCLocalServer";

    public long PrefId { get; set; }

    public string Pref { get; set; } = "nesys";

    public long MessageId { get; set; }
    
    public long MatchTimeout { get; set; } = 99;

    public long MatchWaitTime { get; set; } = 10;

    public long MatchRemainingTime { get; set; } = 89;

}