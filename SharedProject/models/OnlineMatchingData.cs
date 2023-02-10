namespace SharedProject.models;

public class OnlineMatchingData
{
    public long MachineId { get; set; }
    
    public long EventId { get; set; }

    public long MatchingId { get; set; }

    public long EntryNo { get; set; }

    public string EntryStart { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    public long Status { get; set; } = 1;

    public long CardId { get; set; }

    public string PlayerName { get; set; } = string.Empty;

    public long AvatarId { get; set; }

    public long TitleId { get; set; }

    public long ClassId { get; set; }

    public long GroupId { get; set; }

    public long TenpoId { get; set; }
    
    public string TenpoName { get; set; } = "1337";

    public long PrefId { get; set; }

    public string Pref { get; set; } = "nesys";

    public long MessageId { get; set; }

    public long MatchingTimeout { get; set; } = 99;

    public long MatchingWaitTime { get; set; } = 10;

    public long MatchingRemainingTime { get; set; } = 3;
}