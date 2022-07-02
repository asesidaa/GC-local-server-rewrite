using System.Xml.Serialization;
using ChoETL;
using SQLite.Net2;

namespace GCLocalServerRewrite.models;

[Table("card_main")]
public class Card : ICardIdModel
{
    [PrimaryKey]
    [Column("card_id")]
    [ChoXmlElementRecordField(FieldName = "card_id")]
    [XmlElement(ElementName = "card_id")]
    public long CardId { get; set; }

    [Column("player_name")]
    [ChoXmlElementRecordField(FieldName = "player_name")]
    [XmlElement(ElementName = "player_name")]
    public string PlayerName { get; set; } = string.Empty;

    [Column("score_i1")]
    [ChoXmlElementRecordField(FieldName = "score_i1")]
    [XmlElement("score_i1")]
    public int Score { get; set; }

    [Column("fcol1")]
    [ChoXmlElementRecordField(FieldName = "fcol1")]
    [XmlElement("fcol1")]
    public int Fcol1 { get; set; }

    [Column("fcol2")]
    [ChoXmlElementRecordField(FieldName = "fcol2")]
    [XmlElement("fcol2")]
    public int Fcol2 { get; set; }

    [Column("fcol3")]
    [ChoXmlElementRecordField(FieldName = "fcol3")]
    [XmlElement("fcol3")]
    public int Fcol3 { get; set; }

    [Column("achieve_status")]
    [ChoXmlElementRecordField(FieldName = "achieve_status")]
    [XmlElement("achieve_status")]
    public string AchieveStatus { get; set; } = string.Empty;

    [Column("created")]
    [ChoXmlElementRecordField(FieldName = "created")]
    [XmlElement("created")]
    public string Created { get; set; } = "2017-01-01 08:00:00";

    [Column("modified")]
    [ChoXmlElementRecordField(FieldName = "modified")]
    [XmlElement("modified")]
    public string Modified { get; set; } = "2017-01-01 08:00:00";
    
    public void SetCardId(long cardId)
    {
        CardId = cardId;
    }
}