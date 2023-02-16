namespace Application.Dto.Game;

public class MusicDto
{
    [XmlAttribute(AttributeName = "id")]
    public int Id { get; set; }
    
    [XmlElement("music_id")]
    public int MusicId { get; set; }
    
    [XmlElement(ElementName = "title")]
    public string Title { get; set; } = string.Empty;

    [XmlElement(ElementName = "artist")] 
    public string Artist { get; set; } = string.Empty;
    
    [XmlElement(ElementName = "release_date")]
    public string ReleaseDate { get; set; } = "2013-01-01 08:00:00";
    
    [XmlElement(ElementName = "end_date")]
    public string EndDate { get; set; } = "2030-01-01 08:00:00";
    
    [XmlElement("new_flag")]
    public int NewFlag { get; set; }
    
    [XmlElement("use_flag")]
    public int UseFlag { get; set; }
    
    [XmlElement("calc_flag")]
    public int CalcFlag { get; set; }
}