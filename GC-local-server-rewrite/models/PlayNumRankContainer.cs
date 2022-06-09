using System.Xml.Serialization;

namespace GCLocalServerRewrite.models;

[XmlRoot("root")]
public class PlayNumRankContainer
{
    [XmlArray("play_num_rank")]
    public List<PlayNumRankRecord> PlayNumRankRecords { get; set; } = new();
    
    [XmlElement("ranking_status")]
    public RankStatus? RankStatus { get; set; }
}