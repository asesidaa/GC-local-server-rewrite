using System.Xml.Serialization;
using SQLite.Net2;

namespace GCLocalServerRewrite.models;

[Table("music_extra")]
public class MusicExtra : Record
{
    [PrimaryKey]
    [Column("music_id")]
    [XmlElement("music_id")]
    public int MusicId { get; set; }

    [Column("use_flag")]
    [XmlElement("use_flag")]
    public int UseFlag { get; set; }
}