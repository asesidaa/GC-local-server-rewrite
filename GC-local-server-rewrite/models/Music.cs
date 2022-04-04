using System.Xml.Serialization;
using SQLite.Net2;

namespace GCLocalServerRewrite.models;

[Table("music_unlock")]
public class Music : Record
{
    [PrimaryKey]
    [Column("music_id")]
    [XmlElement("music_id")]
    public int MusicId { get; set; }

    [Column("title")]
    [XmlElement(ElementName = "title", IsNullable = true)]
    public string? Title { get; set; }

    [Column("artist")]
    [XmlElement(ElementName = "artist", IsNullable = true)]
    public string? Artist { get; set; }

    [Column("release_date")]
    [XmlElement(ElementName = "release_date", IsNullable = true)]
    public string? ReleaseDate { get; set; }

    [Column("end_date")]
    [XmlElement(ElementName = "end_date", IsNullable = true)]
    public string? EndDate { get; set; }

    [Column("new_flag")]
    [XmlElement("new_flag")]
    public int NewFlag { get; set; }

    [Column("use_flag")]
    [XmlElement("use_flag")]
    public int UseFlag { get; set; }
}