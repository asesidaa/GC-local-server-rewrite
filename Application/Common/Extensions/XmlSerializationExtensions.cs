using ChoETL;
using Throw;

namespace Application.Common;

public static class XmlSerializationExtensions
{
    public static T DeserializeCardData<T>(this string source) where T : class
    {
        using var reader = new ChoXmlReader<T>(new StringReader(source)).WithXPath("/root/data");

        var result = reader.Read();
        result.ThrowIfNull();

        return result;
    }
}