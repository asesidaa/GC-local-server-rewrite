using System.Xml.Serialization;

namespace GCLocalServerRewrite.models;

public class UnlockReward : Record
{
    [XmlElement(ElementName = "card_id")]
    public long CardId { get; set; }

    [XmlElement(ElementName = "reward_id")]
    public int RewardId { get; set; }

    [XmlElement(ElementName = "reward_type")]
    public int RewardType { get; set; }

    [XmlElement(ElementName = "open_date")]
    public string? OpenDate { get; set; } = "2021-05-30";

    [XmlElement(ElementName = "close_date")]
    public string? CloseDate { get; set; } = "2030-05-30";

    [XmlElement(ElementName = "open_time")]
    public string? OpenTime { get; set; } = "00:00:01";

    [XmlElement(ElementName = "close_time")]
    public string? CloseTime { get; set; } = "23:59:59";

    [XmlElement(ElementName = "target_id")]
    public int TargetId { get; set; }

    [XmlElement(ElementName = "target_num")]
    public int TargetNum { get; set; }

    [XmlElement(ElementName = "key_num")]
    public int KeyNum { get; set; }

    [XmlElement("display_flag")]
    public int DisplayFlag { get; set; } = 1;

    [XmlElement("use_flag")]
    public int UseFlag { get; set; } = 1;

    [XmlElement("limited_flag")]
    public int LimitedFlag { get; set; }

    [XmlElement("open_unixtime")]
    public long OpenUnixTime { get; set; } = 1622304001;

    [XmlElement("close_unixtime")]
    public long CloseUnixTime { get; set; } = 1906387199;

    [XmlElement("created")]
    public string? Created { get; set; } = "1";

    [XmlElement("modified")]
    public string? Modified { get; set; } = "1";
}