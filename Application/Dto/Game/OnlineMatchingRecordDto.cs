namespace Application.Dto.Game;

public sealed class OnlineMatchingRecordDto
{
    [XmlAttribute(AttributeName = "id")]
    public int Id { get; set; }

    [XmlElement(ElementName = "card_id")]
    public long CardId { get; set; }

    [XmlElement(ElementName = "player_name")]
    public string PlayerName { get; set; } = string.Empty;

    [XmlElement(ElementName = "machine_id")]
    public uint MachineId { get; set; }

    [XmlElement(ElementName = "event_id")]
    public uint EventId { get; set; }

    [XmlElement(ElementName = "avatar_id")]
    public uint AvatarId { get; set; }

    [XmlElement(ElementName = "title_id")]
    public uint TitleId { get; set; }

    [XmlElement(ElementName = "class_id")]
    public uint ClassId { get; set; }

    [XmlElement(ElementName = "group_id")]
    public uint GroupId { get; set; }

    [XmlElement(ElementName = "tenpo_id")]
    public uint TenpoId { get; set; }

    [XmlElement(ElementName = "tenpo_name")]
    public string TenpoName { get; set; } = string.Empty;

    [XmlElement(ElementName = "pref_id")]
    public uint PrefId { get; set; }

    [XmlElement(ElementName = "pref")]
    public string Pref { get; set; } = string.Empty;

    [XmlElement(ElementName = "message_id")]
    public uint MessageId { get; set; }

    [XmlElement(ElementName = "matching_id")]
    public uint MatchingId { get; set; }

    [XmlElement(ElementName = "entry_no")]
    public uint EntryNumber { get; set; }

    [XmlElement(ElementName = "status")]
    public uint Status { get; set; }

    [XmlElement(ElementName = "matching_timeout")]
    public uint MatchingTimeout { get; set; }

    [XmlElement(ElementName = "matching_wait_time")]
    public uint MatchingWaitTime { get; set; }

    [XmlElement(ElementName = "matching_remaining_time")]
    public uint MatchingRemainingTime { get; set; }

    [XmlElement(ElementName = "entry_start")]
    public string EntryStart { get; set; } = string.Empty;
}
