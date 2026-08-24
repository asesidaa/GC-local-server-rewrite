namespace Application.Dto.Game;

public sealed class OnlineMatchingEntryRequestDto
{
    [XmlElement(ElementName = "machine_id")]
    public long MachineId { get; set; }

    [XmlElement(ElementName = "event_id")]
    public long EventId { get; set; }

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
    public long TenpoId { get; set; }

    [XmlElement(ElementName = "tenpo_name")]
    public string TenpoName { get; set; } = string.Empty;

    [XmlElement(ElementName = "pref_id")]
    public long PrefId { get; set; }

    [XmlElement(ElementName = "pref")]
    public string Pref { get; set; } = string.Empty;

    [XmlElement(ElementName = "message_id")]
    public long MessageId { get; set; }
}
