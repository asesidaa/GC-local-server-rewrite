using System.Xml.Serialization;
using ChoETL;

namespace GCLocalServerRewrite.models;

public class Record
{
    [XmlAttribute(AttributeName = "id")]
    public int RecordId { get; set; }
}