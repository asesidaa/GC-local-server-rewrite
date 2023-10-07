namespace Application.Game.Rank;

[XmlRoot("root")]
public class TenpoScoreRankContainer
{
    [XmlArray(ElementName = "t_score_rank")]
    [XmlArrayItem(ElementName = "record")]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public List<ScoreRankDto> Ranks { get; init; } = new();

    [XmlElement("ranking_status")] 
    public RankStatus Status { get; set; } = new();
}