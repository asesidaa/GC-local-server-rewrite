namespace Application.Dto.Game;

public class ItemDto
{
    [XmlAttribute(AttributeName = "id")]
    public int Id { get; set; }
    
    [XmlElement(ElementName = "card_id")]
    public long CardId { get; set; }

    [XmlElement(ElementName = "item_id")]
    public int ItemId { get; set; }

    [XmlElement(ElementName = "item_num")]
    public int ItemNum { get; set; } = 90;

    [XmlElement("created")]
    public string Created { get; set; } = "1";

    [XmlElement("modified")]
    public string Modified { get; set; } = "1";

    [XmlElement("new_flag")]
    public int NewFlag { get; set; }

    [XmlElement("use_flag")]
    public int UseFlag { get; set; } = 1;
}