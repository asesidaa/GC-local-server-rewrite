using System.Xml.Serialization;

namespace GCLocalServerRewrite.models;

[XmlType("record")]
public class PlayNumRankRecord
{
    [XmlAttribute(AttributeName = "id")]
    public int Id { get; set; }

    [XmlElement("rank")]
    public int Rank { get; set; }

    [XmlElement("rank2")]
    public int Rank2 { get; set; }

    [XmlElement("prev_rank")]
    public int PrevRank { get; set; }

    [XmlElement("prev_rank2")]
    public int PrevRank2 { get; set; }

    [XmlElement("pcol1")]
    public int Pcol1 { get; set; }

    [XmlElement("score_bi1")]
    public int ScoreBi1 { get; set; }

    [XmlElement("title")]
    public string? Title { get; set; }

    [XmlElement("artist")]
    public string? Artist { get; set; }
}