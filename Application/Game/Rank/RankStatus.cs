namespace Application.Game.Rank;

public class RankStatus
{
    [XmlElement("table_name")] 
    public string TableName { get; set; } = string.Empty;

    [XmlElement("start_date")]
    public string StartDate { get; set; } = string.Empty;

    [XmlElement("end_date")]
    public string EndDate { get; set; } = string.Empty;

    [XmlElement("status")]
    public int Status { get; set; }

    [XmlElement("rows")]
    public int Rows { get; set; }
}