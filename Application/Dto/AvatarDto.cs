using System.ComponentModel;
using System.Xml.Serialization;

namespace Application.Dto;

public class AvatarDto
{
    [XmlAttribute(AttributeName = "id")]
    public int Id { get; set; }
    
    [XmlElement(ElementName = "card_id")]
    public long CardId { get; set; }

    [XmlElement(ElementName = "avatar_id")]
    public int AvatarId { get; set; }

    [XmlElement("created")]
    [DefaultValue("")]
    public string Created { get; set; } = string.Empty;

    [XmlElement("modified")]
    [DefaultValue("")]
    public string Modified { get; set; } = string.Empty;

    [XmlElement("new_flag")]
    public int NewFlag { get; set; }

    [XmlElement("use_flag")]
    public int UseFlag { get; set; }
}