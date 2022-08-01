using System.Xml.Serialization;
using ChoETL;

namespace GCLocalServerRewrite.models;

public class OnlineMatchingEntry: Record
{
    [XmlElement(ElementName = "machine_id")]
    [ChoXmlElementRecordField(FieldName = "machine_id")]
    public long MachineId { get; set; }
    
    [XmlElement(ElementName = "event_id")]
    [ChoXmlElementRecordField(FieldName = "event_id")]
    public long EventId { get; set; }
    
    [XmlElement(ElementName = "matching_id")]
    [ChoXmlElementRecordField(FieldName = "matching_id")]
    public long MatchingId { get; set; }
    
    [XmlElement(ElementName = "entry_no")]
    [ChoXmlElementRecordField(FieldName = "entry_no")]
    public long EntryNo { get; set; }
    
    [XmlElement(ElementName = "entry_start")]
    [ChoXmlElementRecordField(FieldName = "entry_start")]
    public string EntryStart { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    [XmlElement(ElementName = "status")]
    [ChoXmlElementRecordField(FieldName = "status")]
    public long Status { get; set; } = 1;
    
    [XmlElement(ElementName = "card_id")]
    [ChoXmlElementRecordField(FieldName = "card_id")]
    public long CardId { get; set; }
    
    [XmlElement(ElementName = "player_name")]
    [ChoXmlElementRecordField(FieldName = "player_name")]
    public string PlayerName { get; set; } = string.Empty;
    
    [XmlElement(ElementName = "avatar_id")]
    [ChoXmlElementRecordField(FieldName = "avatar_id")]
    public long AvatarId { get; set; }
    
    [XmlElement(ElementName = "title_id")]
    [ChoXmlElementRecordField(FieldName = "title_id")]
    public long TitleId { get; set; }
    
    [XmlElement(ElementName = "class_id")]
    [ChoXmlElementRecordField(FieldName = "class_id")]
    public long ClassId { get; set; }
    
    [XmlElement(ElementName = "group_id")]
    [ChoXmlElementRecordField(FieldName = "group_id")]
    public long GroupId { get; set; }
    
    [XmlElement(ElementName = "tenpo_id")]
    [ChoXmlElementRecordField(FieldName = "tenpo_id")]
    public long TenpoId { get; set; }
    
    [XmlElement(ElementName = "tenpo_name")]
    [ChoXmlElementRecordField(FieldName = "tenpo_name")]
    public string TenpoName { get; set; } = "1337";
    
    [XmlElement(ElementName = "pref_id")]
    [ChoXmlElementRecordField(FieldName = "pref_id")]
    public long PrefId { get; set; }
    
    [XmlElement(ElementName = "pref")]
    [ChoXmlElementRecordField(FieldName = "pref")]
    public string Pref { get; set; } = "nesys";
    
    [XmlElement(ElementName = "message_id")]
    [ChoXmlElementRecordField(FieldName = "message_id")]
    public long MessageId { get; set; }

    /// <summary>
    /// Communication timeout?
    /// </summary>
    [XmlElement(ElementName = "matching_timeout")]
    [ChoXmlElementRecordField(FieldName = "matching_timeout")]
    public long MatchingTimeout { get; set; } = 99;

    /// <summary>
    /// Wait time
    /// </summary>
    [XmlElement(ElementName = "matching_wait_time")]
    [ChoXmlElementRecordField(FieldName = "matching_wait_time")]
    public long MatchingWaitTime { get; set; } = 10;

    /// <summary>
    /// Seems not used
    /// </summary>
    [XmlElement(ElementName = "matching_remaining_time")]
    [ChoXmlElementRecordField(FieldName = "matching_remaining_time")]
    public long MatchingRemainingTime { get; set; } = 89;
}