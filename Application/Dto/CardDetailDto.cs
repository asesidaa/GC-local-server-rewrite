using System.ComponentModel;

namespace Application.Dto;

public class CardDetailDto
{
    [XmlAttribute(AttributeName = "id")] 
    public int Id { get; set; } = -1;

    public bool ShouldSerializeId()
    {
        return Id != -1;
    }
    
    [XmlElement(ElementName = "card_id")]
    public long CardId { get; set; }
    
    [XmlElement(ElementName = "pcol1")]
    public int Pcol1 { get; set; }
    
    [XmlElement(ElementName = "pcol2")]
    public int Pcol2 { get; set; }
    
    [XmlElement(ElementName = "pcol3")]
    public int Pcol3 { get; set; }

    [XmlElement(ElementName = "score_i1")]
    public long ScoreI1 { get; set; }
    
    [XmlElement(ElementName = "score_ui1")]
    public long ScoreUi1 { get; set; }

    [XmlElement(ElementName = "score_ui2")]
    public long ScoreUi2 { get; set; }

    [XmlElement(ElementName = "score_ui3")]
    public long ScoreUi3 { get; set; }

    [XmlElement(ElementName = "score_ui4")]
    public long ScoreUi4 { get; set; }

    [XmlElement(ElementName = "score_ui5")]
    public long ScoreUi5 { get; set; }

    [XmlElement(ElementName = "score_ui6")]
    public long ScoreUi6 { get; set; }

    [XmlElement(ElementName = "score_bi1")]
    public long ScoreBi1 { get; set; }

    [XmlElement(ElementName = "last_play_tenpo_id")]
    [DefaultValue("1337")]
    public string LastPlayTenpoId { get; set; } = "1337";

    [XmlElement("fcol1")]
    public int Fcol1 { get; set; }
    
    [XmlElement("fcol2")]
    public int Fcol2 { get; set; }

    [XmlElement("fcol3")]
    public int Fcol3 { get; set; }
    
    [XmlIgnore]
    public DateTime LastPlayTime { get; set; } = DateTime.MinValue;
}