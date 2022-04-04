using System.Xml.Serialization;
using ChoETL;

namespace GCLocalServerRewrite.models;

[ChoXmlRecordObject(XPath = "/root/data")]
public class CardDetailReadData
{
    [XmlElement(ElementName = "pcol1")]
    [ChoXmlElementRecordField(FieldName = "pcol1")]
    public int Pcol1 { get; set; }

    [XmlElement(ElementName = "pcol2")]
    [ChoXmlElementRecordField(FieldName = "pcol2")]
    public int Pcol2 { get; set; }

    [XmlElement(ElementName = "pcol3")]
    [ChoXmlElementRecordField(FieldName = "pcol3")]
    public int Pcol3 { get; set; }
}