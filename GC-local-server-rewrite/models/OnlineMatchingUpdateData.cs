using ChoETL;

namespace GCLocalServerRewrite.models;

[ChoXmlRecordObject(XPath = "/root/data")]
// ReSharper disable once ClassNeverInstantiated.Global
public class OnlineMatchingUpdateData
{
    [ChoXmlElementRecordField(FieldName = "action")]
    public long Action { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "event_id")]
    public long EventId { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "matching_id")]
    public long MatchingId { get; set; }
    
    [ChoXmlElementRecordField(FieldName = "message_id")]
    public long MessageId { get; set; }
}