using System.Xml.Serialization;

namespace Application.Dto;

public class CardDto
{
    [XmlElement(ElementName = "card_id")]
    public long CardId { get; set; }

    [XmlElement(ElementName = "player_name")]
    public string PlayerName { get; set; } = string.Empty;

    [XmlElement("score_i1")]
    public long ScoreI1 { get; set; }

    [XmlElement("fcol1")]
    public long Fcol1 { get; set; }

    [XmlElement("fcol2")]
    public long Fcol2 { get; set; }

    [XmlElement("fcol3")]
    public long Fcol3 { get; set; }

    [XmlElement("achieve_status")]
    public string AchieveStatus { get; set; } = string.Empty;

    [XmlElement("created")]
    public string Created { get; set; } = string.Empty;

    [XmlElement("modified")]
    public string Modified { get; set; } = string.Empty;
}