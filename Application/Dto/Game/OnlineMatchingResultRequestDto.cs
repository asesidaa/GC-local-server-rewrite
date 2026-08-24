namespace Application.Dto.Game;

public sealed class OnlineMatchingResultRequestDto
{
    [XmlElement(ElementName = "event_id")]
    public long EventId { get; set; }

    [XmlElement(ElementName = "matching_id")]
    public long MatchingId { get; set; }

    [XmlElement(ElementName = "class_id")]
    public long ClassId { get; set; }

    [XmlElement(ElementName = "group_id")]
    public long GroupId { get; set; }

    [XmlElement(ElementName = "result_score")]
    public long ResultScore { get; set; }

    [XmlElement(ElementName = "result_star")]
    public long ResultStar { get; set; }

    [XmlElement(ElementName = "result_rank")]
    public long ResultRank { get; set; }

    [XmlElement(ElementName = "music_id_1")]
    public long MusicId1 { get; set; }

    [XmlElement(ElementName = "music_id_2")]
    public long MusicId2 { get; set; }

    [XmlElement(ElementName = "music_id_3")]
    public long MusicId3 { get; set; }

    [XmlElement(ElementName = "difficulty_lv_1")]
    public long DifficultyLevel1 { get; set; }

    [XmlElement(ElementName = "difficulty_lv_2")]
    public long DifficultyLevel2 { get; set; }

    [XmlElement(ElementName = "difficulty_lv_3")]
    public long DifficultyLevel3 { get; set; }

    [XmlElement(ElementName = "item_id_1")]
    public long ItemId1 { get; set; }

    [XmlElement(ElementName = "item_id_2")]
    public long ItemId2 { get; set; }

    [XmlElement(ElementName = "item_id_3")]
    public long ItemId3 { get; set; }
}
