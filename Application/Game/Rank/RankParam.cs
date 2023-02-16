using System.ComponentModel;
using System.Xml.Serialization;

namespace Application.Game.Rank;

public class RankParam
{
    [XmlElement(ElementName = "card_id")]
    [DefaultValue("0")]
    public long CardId { get; set; }
}