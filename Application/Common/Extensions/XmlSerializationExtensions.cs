using System.Collections.Concurrent;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Throw;

namespace Application.Common.Extensions;

public static class XmlSerializationExtensions
{
    private static readonly XmlSerializerNamespaces EmptyNamespaces = new([XmlQualifiedName.Empty]);

    // XmlSerializer with a custom XmlRootAttribute is NOT cached by the runtime (unlike the default
    // constructor), and each construction emits a new dynamic assembly that is never collected.
    // Cache them here to prevent unbounded memory growth.
    private static readonly ConcurrentDictionary<(Type, string), XmlSerializer> SerializerCache = new();

    private static XmlSerializer GetCachedSerializer(Type type, string rootName) =>
        SerializerCache.GetOrAdd((type, rootName), key => new XmlSerializer(key.Item1, new XmlRootAttribute(key.Item2)));

    public static T DeserializeCardData<T>(this string source) where T : class
    {
        var doc = XDocument.Parse(source);
        var dataElement = doc.Root?.Element("data");
        dataElement.ThrowIfNull();

        var serializer = GetCachedSerializer(typeof(T), "data");
        using var reader = dataElement.CreateReader();
        var result = serializer.Deserialize(reader) as T;
        result.ThrowIfNull();

        return result;
    }

    public static string SerializeCardData<T>(this T source, string xpath) where T : class
    {
        var innerXml = SerializeToFragment(source);
        return WrapInXPath(innerXml, xpath);
    }

    public static string SerializeCardData<T>(this T source) where T : class
    {
        // Serialize using the class's own [XmlRoot] attribute
        var serializer = new XmlSerializer(typeof(T));
        var settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = false,
            Indent = true,
            Encoding = new UTF8Encoding(false)
        };

        using var stream = new MemoryStream();
        using (var writer = XmlWriter.Create(stream, settings))
        {
            serializer.Serialize(writer, source, EmptyNamespaces);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    public static string SerializeCardDataList<T>(this IEnumerable<T> source, string xpath) where T : class
    {
        var fragments = new StringBuilder();
        foreach (var item in source)
        {
            fragments.Append(SerializeToFragment(item));
        }

        return WrapInXPath(fragments.ToString(), xpath);
    }

    /// <summary>
    /// Serializes an object to an XML fragment string (just the element content, no XML declaration).
    /// The root element name comes from the XPath leaf node, so we strip the serializer's root here.
    /// </summary>
    private static string SerializeToFragment<T>(T source) where T : class
    {
        var serializer = new XmlSerializer(typeof(T));
        var settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            Indent = true
        };

        using var stringWriter = new StringWriter();
        using (var writer = XmlWriter.Create(stringWriter, settings))
        {
            serializer.Serialize(writer, source, EmptyNamespaces);
        }

        return stringWriter.ToString();
    }

    /// <summary>
    /// Wraps serialized XML content in an XPath-defined element hierarchy.
    /// For example, xpath "/root/card" with content "&lt;card_id&gt;1&lt;/card_id&gt;" produces:
    /// &lt;root&gt;&lt;card&gt;&lt;card_id&gt;1&lt;/card_id&gt;&lt;/card&gt;&lt;/root&gt;
    ///
    /// The xpath leaf is used as the wrapper for each serialized item's content.
    /// The serialized item's root element name (from XmlSerializer) is replaced by the xpath leaf.
    /// </summary>
    private static string WrapInXPath(string serializedContent, string xpath)
    {
        // Parse xpath like "/root/card" or "/root/music/record" into element names
        var parts = xpath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            throw new ArgumentException($"XPath must have at least 2 levels (e.g. /root/card), got: {xpath}");
        }

        // The serialized content has a root element from XmlSerializer (e.g. <CardDto>...</CardDto>).
        // We need to replace that with the xpath leaf element name (e.g. <card>...</card>).
        // Parse the serialized content and rename the root elements.
        var renamedContent = RenameRootElements(serializedContent, parts[^1]);

        // Build the wrapping hierarchy from outside in
        // For "/root/card": wrap renamedContent in <root>...<card>...
        // For "/root/music/record": wrap renamedContent in <root><music>...
        var doc = new XDocument();
        XElement? outermost = null;
        XElement? innermost = null;

        // Build all levels except the leaf (which is the renamed element itself)
        for (var i = 0; i < parts.Length - 1; i++)
        {
            var element = new XElement(parts[i]);
            if (outermost is null)
            {
                outermost = element;
                innermost = element;
            }
            else
            {
                innermost!.Add(element);
                innermost = element;
            }
        }

        // Add the renamed content inside the innermost wrapper
        // Parse as XML fragment and add
        var wrappedXml = $"<_wrapper_>{renamedContent}</_wrapper_>";
        var fragment = XElement.Parse(wrappedXml);
        foreach (var child in fragment.Elements())
        {
            innermost!.Add(child);
        }

        doc.Add(outermost);
        doc.Declaration = new XDeclaration("1.0", "utf-8", null);

        using var stream = new MemoryStream();
        var settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = false,
            Indent = true,
            Encoding = new UTF8Encoding(false)
        };
        using (var writer = XmlWriter.Create(stream, settings))
        {
            doc.WriteTo(writer);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// Renames the root element(s) in serialized XML content to the specified name.
    /// Handles both single elements and multiple concatenated elements.
    /// </summary>
    private static string RenameRootElements(string serializedContent, string newName)
    {
        // Wrap in a temp root to handle multiple elements (list serialization)
        var wrappedXml = $"<_wrapper_>{serializedContent}</_wrapper_>";
        var wrapper = XElement.Parse(wrappedXml);

        foreach (var child in wrapper.Elements())
        {
            child.Name = newName;
        }

        var sb = new StringBuilder();
        foreach (var child in wrapper.Elements())
        {
            sb.Append(child);
        }

        return sb.ToString();
    }
}
