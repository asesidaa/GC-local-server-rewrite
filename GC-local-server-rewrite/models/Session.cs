using System.Xml.Serialization;

namespace GCLocalServerRewrite.models;

public class Session
{
    [XmlElement(ElementName = "card_id")]
    public long CardId { get; set; }

    [XmlElement(ElementName = "mac_addr")]
    public string? Mac { get; set; }

    [XmlElement(ElementName = "session_id")]
    public string? SessionId { get; set; }

    [XmlElement(ElementName = "expires")]
    public int Expires { get; set; }

    [XmlElement(ElementName = "player_id")]
    public int PlayerId { get; set; }
}