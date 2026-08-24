namespace Application.Dto.Game;

public sealed class OnlineMatchingUpdateRequestDto
{
    [XmlElement(ElementName = "action")]
    public long Action { get; set; }

    [XmlElement(ElementName = "event_id")]
    public long EventId { get; set; }

    [XmlElement(ElementName = "matching_id")]
    public long MatchingId { get; set; }

    [XmlElement(ElementName = "message_id")]
    public long MessageId { get; set; }
}
