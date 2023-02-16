namespace Application.Dto.Game;

public class SessionDto
{
    [XmlElement(ElementName = "card_id")]
    public long CardId { get; set; }

    [XmlElement(ElementName = "mac_addr")] 
    public string Mac { get; set; } = "000000000000";

    [XmlElement(ElementName = "session_id")]
    public string SessionId { get; set; } = "12345678901234567890123456789012";

    [XmlElement(ElementName = "expires")]
    public int Expires { get; set; }

    [XmlElement(ElementName = "player_id")]
    public int PlayerId { get; set; }
}