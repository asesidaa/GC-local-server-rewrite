using ChoETL;

namespace GCLocalServerRewrite.models;

[ChoXmlRecordObject(XPath = "/root/data")]
public class OnlineMatchingResultData
{
    [ChoXmlElementRecordField(FieldName = "event_id")]
    public int EventId { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "matching_id")]
    public int MatchingId { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "class_id")]
    public int ClassId { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "group_id")]
    public int GroupId { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "result_score")]
    public int ResultScore { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "result_star")]
    public int ResultStar { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "result_rank")]
    public int ResultRank { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "music_id_1st")]
    public int MusicIdFirst { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "music_id_2nd")]
    public int MusicIdSecond { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "music_id_3rd")]
    public int MusicIdThird { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "difficulty_lv_1st")]
    public int DifficultyFirst { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "difficulty_lv_2nd")]
    public int DifficultySecond { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "difficulty_lv_3rd")]
    public int DifficultyThird { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "item_id_1st")]
    public int ItemIdFirst { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "item_id_2nd")]
    public int ItemIdSecond { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "item_id_3rd")]
    public int ItemIdThird { get; set; }

}