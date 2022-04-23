using System.Xml.Serialization;
using SQLite.Net2;

namespace GCLocalServerRewrite.models;

public class Record
{
    [XmlAttribute(AttributeName = "id")]
    [Ignore]
    public int RecordId { get; set; }
}