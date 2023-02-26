namespace Application.Dto.Game;

public class OnlineMatchEntryDto
{
    [XmlAttribute(AttributeName = "id")]
    public int Id { get; set; }
    
    [XmlElement(ElementName = "machine_id")]
    public long MachineId { get; set; }
    
    [XmlElement(ElementName = "event_id")]
    public long EventId { get; set; }
    
    [XmlElement(ElementName = "matching_id")]
    public long MatchId { get; set; }
    
    [XmlElement(ElementName = "entry_no")]
    public long EntryId { get; set; }
    
    [XmlElement(ElementName = "entry_start")]
    public string StartTime { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    [XmlElement(ElementName = "status")]
    public long Status { get; set; } = 1;
    
    [XmlElement(ElementName = "card_id")]
    public long CardId { get; set; }
    
    [XmlElement(ElementName = "player_name")]
    public string PlayerName { get; set; } = string.Empty;
    
    [XmlElement(ElementName = "avatar_id")]
    public long AvatarId { get; set; }
    
    [XmlElement(ElementName = "title_id")]
    public long TitleId { get; set; }
    
    [XmlElement(ElementName = "class_id")]
    public long ClassId { get; set; }
    
    [XmlElement(ElementName = "group_id")]
    public long GroupId { get; set; }

    [XmlElement(ElementName = "tenpo_id")] 
    public long TenpoId { get; set; } = 1337;
    
    [XmlElement(ElementName = "tenpo_name")]
    public string TenpoName { get; set; } = "GCLocalServer";
    
    [XmlElement(ElementName = "pref_id")]
    public long PrefId { get; set; }
    
    [XmlElement(ElementName = "pref")]
    public string Pref { get; set; } = "nesys";
    
    [XmlElement(ElementName = "message_id")]
    public long MessageId { get; set; }
    
    [XmlElement(ElementName = "matching_timeout")]
    public long MatchTimeout { get; set; } = 99;

    [XmlElement(ElementName = "matching_wait_time")]
    public long MatchWaitTime { get; set; } = 10;

    [XmlElement(ElementName = "matching_remaining_time")]
    public long MatchRemainingTime { get; set; } = 89;
}