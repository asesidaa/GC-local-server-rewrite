using System.Xml.Serialization;

namespace Application.Dto;

public class UnlockKeyNumDto
{
    [XmlAttribute(AttributeName = "id")]
    public int Id { get; set; }
    
    [XmlElement(ElementName = "card_id")]
    public long CardId { get; set; }

    [XmlElement(ElementName = "reward_id")]
    public int RewardId { get; set; }

    [XmlElement(ElementName = "key_num")]
    public int KeyNum { get; set; }

    [XmlElement(ElementName = "reward_count")]
    public int RewardCount { get; set; }

    [XmlElement("expired_flag")]
    public int ExpiredFlag { get; set; }

    [XmlElement("use_flag")]
    public int UseFlag { get; set; }

    [XmlElement("cash_flag")]
    public int CashFlag { get; set; }

    [XmlElement("created")]
    public string Created { get; set; } = string.Empty;

    [XmlElement("modified")] 
    public string Modified { get; set; } = string.Empty;
}