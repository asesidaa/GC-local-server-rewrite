using System.Xml.Serialization;

namespace Application.Dto;

public class SkinDto
{
    [XmlAttribute(AttributeName = "id")]
    public int Id { get; set; }
    
    [XmlElement(ElementName = "card_id")]
    public long CardId { get; set; }

    [XmlElement(ElementName = "skin_id")]
    public int SkinId { get; set; }

    [XmlElement("created")]
    public string Created { get; set; } = string.Empty;

    [XmlElement("modified")]
    public string Modified { get; set; } = string.Empty;

    [XmlElement("new_flag")]
    public int NewFlag { get; set; }

    [XmlElement("use_flag")]
    public int UseFlag { get; set; }
}