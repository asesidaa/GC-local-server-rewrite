using ChoETL;

namespace GCLocalServerRewrite.models;

[ChoXmlRecordObject(XPath = "/root/data")]
public class OnlineMatchingResultData
{
    [ChoXmlElementRecordField(FieldName = "event_id")]
    public long EventId { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "matching_id")]
    public long MatchingId { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "class_id")]
    public long ClassId { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "group_id")]
    public long GroupId { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "result_score")]
    public long ResultScore { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "result_star")]
    public long ResultStar { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "result_rank")]
    public long ResultRank { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "music_id_1st")]
    public long MusicIdFirst { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "music_id_2nd")]
    public long MusicIdSecond { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "music_id_3rd")]
    public long MusicIdThird { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "difficulty_lv_1st")]
    public long DifficultyFirst { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "difficulty_lv_2nd")]
    public long DifficultySecond { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "difficulty_lv_3rd")]
    public long DifficultyThird { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "item_id_1st")]
    public long ItemIdFirst { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "item_id_2nd")]
    public long ItemIdSecond { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "item_id_3rd")]
    public long ItemIdThird { get; set; }

}