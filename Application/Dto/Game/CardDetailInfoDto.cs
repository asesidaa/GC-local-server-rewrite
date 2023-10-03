using System.ComponentModel;

namespace Application.Dto.Game;

public class CardDetailInfoDto
{
    [XmlAttribute(AttributeName = "id")] 
    public int Id { get; set; } = -1;

    public bool ShouldSerializeId()
    {
        return Id != -1;
    }
    
    [XmlElement(ElementName = "card_id")]
    public long CardId { get; set; }
    
    [XmlElement(ElementName = "tenpo_id")]
    public string TenpoId { get; set; }
    
    [XmlElement(ElementName = "tenpo_name")]
    public string TenpoName { get; set; }
    
    [XmlElement(ElementName = "pref_name")]
    public string PrefName { get; set; }

    [XmlElement(ElementName = "tenpo_ip_addr")]
    public string TenpoIpAddr { get; set; }
    
    [XmlElement(ElementName = "local_ip_addr")]
    public string LocalIpAddr { get; set; }

    [XmlElement(ElementName = "mac_addr")]
    public string MacAddr { get; set; }

    [XmlElement(ElementName = "game_id")]
    public string GameId { get; set; } 

}