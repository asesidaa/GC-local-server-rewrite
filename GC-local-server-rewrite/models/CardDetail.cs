using System.Xml.Serialization;
using ChoETL;
using SQLite.Net2;

namespace GCLocalServerRewrite.models;

[Table("card_detail")]
public class CardDetail : Record, ICardIdModel
{
    [PrimaryKey]
    [Column("card_id")]
    [ChoXmlElementRecordField(FieldName = "card_id")]
    [XmlElement(ElementName = "card_id")]
    public long CardId { get; set; }

    [PrimaryKey]
    [Column("pcol1")]
    [ChoXmlElementRecordField(FieldName = "pcol1")]
    [XmlElement(ElementName = "pcol1")]
    public int Pcol1 { get; set; }

    [PrimaryKey]
    [Column("pcol2")]
    [ChoXmlElementRecordField(FieldName = "pcol2")]
    [XmlElement(ElementName = "pcol2")]
    public int Pcol2 { get; set; }

    [PrimaryKey]
    [Column("pcol3")]
    [ChoXmlElementRecordField(FieldName = "pcol3")]
    [XmlElement(ElementName = "pcol3")]
    public int Pcol3 { get; set; }

    [Column("score_i1")]
    [ChoXmlElementRecordField(FieldName = "score_i1")]
    [XmlElement(ElementName = "score_i1")]
    public long ScoreI1 { get; set; }

    [Column("score_ui1")]
    [ChoXmlElementRecordField(FieldName = "score_ui1")]
    [XmlElement(ElementName = "score_ui1")]
    public long ScoreUi1 { get; set; }

    [Column("score_ui2")]
    [ChoXmlElementRecordField(FieldName = "score_ui2")]
    [XmlElement(ElementName = "score_ui2")]
    public long ScoreUi2 { get; set; }

    [Column("score_ui3")]
    [ChoXmlElementRecordField(FieldName = "score_ui3")]
    [XmlElement(ElementName = "score_ui3")]
    public long ScoreUi3 { get; set; }

    [Column("score_ui4")]
    [ChoXmlElementRecordField(FieldName = "score_ui4")]
    [XmlElement(ElementName = "score_ui4")]
    public long ScoreUi4 { get; set; }

    [Column("score_ui5")]
    [ChoXmlElementRecordField(FieldName = "score_ui5")]
    [XmlElement(ElementName = "score_ui5")]
    public long ScoreUi5 { get; set; }

    [Column("score_ui6")]
    [ChoXmlElementRecordField(FieldName = "score_ui6")]
    [XmlElement(ElementName = "score_ui6")]
    public long ScoreUi6 { get; set; }

    [Column("score_bi1")]
    [ChoXmlElementRecordField(FieldName = "score_bi1")]
    [XmlElement(ElementName = "score_bi1")]
    public long ScoreBi1 { get; set; }

    [Column("last_play_tenpo_id")]
    [ChoXmlElementRecordField(FieldName = "last_play_tenpo_id")]
    [XmlElement(ElementName = "last_play_tenpo_id")]
    public string LastPlayShopId { get; set; } = "1337";

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
    
    [Column("last_play_time")]
    [XmlIgnore]
    public DateTime LastPlayTime { get; set; } = DateTime.MinValue;

    public void SetCardId(long cardId)
    {
        CardId = cardId;
    }
}