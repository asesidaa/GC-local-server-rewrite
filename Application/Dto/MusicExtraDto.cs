using System.Xml.Serialization;

namespace Application.Dto;

public class MusicExtraDto
{
    [XmlAttribute(AttributeName = "id")]
    public int Id { get; set; }
    
    [XmlElement(ElementName = "music_id")]
    public int MusicId { get; set; }
    
    [XmlElement(ElementName = "use_flag")]
    public int UseFlag { get; set; }
}