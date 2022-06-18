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
    [XmlElement(ElementName = "title")]
    public string? Title { get; set; } = string.Empty;

    [Column("artist")]
    [XmlElement(ElementName = "artist")]
    public string? Artist
    {
        get => _artist;
        set => _artist = value ?? string.Empty;
    } 

    private string? _artist = string.Empty;

    [Column("release_date")]
    [XmlElement(ElementName = "release_date")]
    public string ReleaseDate { get; set; } = "2013-01-01 08:00:00";

    [Column("end_date")]
    [XmlElement(ElementName = "end_date")]
    public string EndDate { get; set; } = "2030-01-01 08:00:00";

    [Column("new_flag")]
    [XmlElement("new_flag")]
    public int NewFlag { get; set; }

    [Column("use_flag")]
    [XmlElement("use_flag")]
    public int UseFlag { get; set; }

    [Ignore]
    [XmlElement("calc_flag")]
    public uint CalcFlag { get; set; } = 2;
}