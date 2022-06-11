using System.Xml.Serialization;

namespace GCLocalServerRewrite.models;

public class Coin
{
    [XmlElement(ElementName = "card_id")]
    public long CardId { get; set; }

    [XmlElement(ElementName = "current")]
    public int CurrentCoins { get; set; }

    [XmlElement(ElementName = "total")]
    public int TotalCoins { get; set; }

    [XmlElement("monthly")]
    public int MonthlyCoins { get; set; }

    [XmlElement("created")]
    public string Created { get; set; } = "1";

    [XmlElement("modified")]
    public string Modified { get; set; } = "1";
}