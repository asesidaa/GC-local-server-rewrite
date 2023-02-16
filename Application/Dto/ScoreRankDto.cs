using System.Xml.Serialization;

namespace Application.Dto;

public class ScoreRankDto
{
    [XmlAttribute(AttributeName = "id")]
    public int Id { get; set; }
    
    [XmlElement(ElementName = "card_id")]
    public long CardId { get; set; }

    [XmlElement(ElementName = "player_name")]
    public string PlayerName { get; set; } = string.Empty;

    [XmlElement(ElementName = "rank")]
    public long Rank { get; set; }

    [XmlElement(ElementName = "rank2")]
    public long Rank2 { get; set; } 

    [XmlElement(ElementName = "score_bi1")]
    public long TotalScore { get; set; }
    
    [XmlElement(ElementName = "score_i1")]
    public int AvatarId { get; set; }

    [XmlElement(ElementName = "fcol2")]
    public long TitleId { get; set; }

    [XmlElement(ElementName = "fcol1")]
    public long Fcol1 { get; set; }

    [XmlElement(ElementName = "pref_id")]
    public int PrefId { get; set; }

    [XmlElement(ElementName = "pref")]
    public string Pref { get; set; } = string.Empty;

    [XmlElement(ElementName = "area_id")]
    public int AreaId { get; set; }

    [XmlElement(ElementName = "area")]
    public string Area { get; set; } = string.Empty;

    [XmlElement(ElementName = "last_play_tenpo_id")]
    public int LastPlayTenpoId { get; set; }

    [XmlElement(ElementName = "tenpo_name")]
    public string TenpoName { get; set; } = string.Empty;

    [XmlElement(ElementName = "title")]
    public string Title { get; set; } = string.Empty;
}