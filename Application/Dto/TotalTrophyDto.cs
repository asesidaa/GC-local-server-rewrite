namespace Application.Dto;

public class TotalTrophyDto
{
    [XmlElement(ElementName = "card_id")]
    public long CardId { get; set; }

    [XmlElement(ElementName = "total_trophy_num")]
    public int TrophyNum { get; set; }
}