using System.Linq.Expressions;
using System.Text;
using ChoETL;
using Throw;

namespace Application.Common.Extensions;

public static class XmlSerializationExtensions
{
    public static T DeserializeCardData<T>(this string source) where T : class
    {
        using var reader = new ChoXmlReader<T>(new StringReader(source)).WithXPath("/root/data");

        var result = reader.Read();
        result.ThrowIfNull();

        return result;
    }

    public static string SerializeCardData<T>(this T source, string xpath) where T : class
    {
        var buffer = new StringBuilder();
        using (var writer = new ChoXmlWriter<T>(buffer).WithXPath(xpath).UseXmlSerialization())
        {
            writer.Configuration.OmitXmlDeclaration = false;
            writer.Configuration.DoNotEmitXmlNamespace = true;
            writer.Write(source);
        }
        return buffer.ToString();
    }
    public static string SerializeCardDataList<T>(this IEnumerable<T> source, string xpath) where T : class
    {
        var buffer = new StringBuilder();
        using (var writer = new ChoXmlWriter<T>(buffer).WithXPath(xpath).UseXmlSerialization())
        {
            writer.Configuration.OmitXmlDeclaration = false;
            writer.Configuration.DoNotEmitXmlNamespace = true;
            writer.Write(source);
        }

        return buffer.ToString();
    }
}