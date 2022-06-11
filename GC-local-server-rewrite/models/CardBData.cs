using System.Xml.Serialization;
using ChoETL;
using SQLite.Net2;

namespace GCLocalServerRewrite.models;

[Table("card_bdata")]
public class CardBData : ICardIdModel
{
    [PrimaryKey]
    [Column("card_id")]
    [ChoXmlElementRecordField(FieldName = "card_id")]
    [XmlElement(ElementName = "card_id")]
    public long CardId { get; set; }

    [Column("bdata")]
    [ChoXmlElementRecordField(FieldName = "bdata")]
    [XmlElement(ElementName = "bdata")]
    public string BData { get; set; } = string.Empty;

    [Column("bdata_size")]
    [ChoXmlElementRecordField(FieldName = "bdata_size")]
    [XmlElement(ElementName = "bdata_size")]
    public int BDataSize { get; set; }

    public void SetCardId(long cardId)
    {
        CardId = cardId;
    }
}