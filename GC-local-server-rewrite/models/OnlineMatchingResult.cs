using System.Xml.Serialization;

namespace GCLocalServerRewrite.models;

public class OnlineMatchingResult
{
    [XmlElement(ElementName = "status")]
    public int Status { get; set; }
}