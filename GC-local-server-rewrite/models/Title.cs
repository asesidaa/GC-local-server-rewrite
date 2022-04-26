using System.Xml.Serialization;

namespace GCLocalServerRewrite.models;

public class Title : Record, IIdModel, ICardIdModel
{
    [XmlElement(ElementName = "card_id")]
    public long CardId { get; set; }

    [XmlElement(ElementName = "title_id")]
    public int TitleId { get; set; }

    [XmlElement("created")]
    public string? Created { get; set; } = "1";

    [XmlElement("modified")]
    public string? Modified { get; set; } = "1";

    [XmlElement("new_flag")]
    public int NewFlag { get; set; }

    [XmlElement("use_flag")]
    public int UseFlag { get; set; } = 1;

    public void SetId(int id)
    {
        TitleId = id;
    }

    public void SetCardId(long cardId)
    {
        CardId = cardId;
    }
}