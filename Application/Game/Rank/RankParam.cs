using System.ComponentModel;

namespace Application.Game.Rank;

public class RankParam
{
    [XmlElement(ElementName = "card_id")]
    [DefaultValue("0")]
    public long CardId { get; set; }
    
    [XmlElement(ElementName = "tenpo_id")]
    [DefaultValue("0")]
    public int TenpoId { get; set; }
}