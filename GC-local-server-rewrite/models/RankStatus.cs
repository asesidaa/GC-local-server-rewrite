using System.Xml.Serialization;

namespace GCLocalServerRewrite.models;

public class RankStatus
{
    [XmlElement("table_name")]
    public string? TableName { get; set; }

    [XmlElement("start_date")]
    public string StartDate { get; set; } = "2017-05-30";

    [XmlElement("end_date")]
    public string EndDate { get; set; } = "2018-05-30";

    [XmlElement("status")]
    public int Status { get; set; }

    [XmlElement("rows")]
    public int Rows { get; set; }
}