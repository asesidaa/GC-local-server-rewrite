using System.Text.Json.Serialization;

namespace DatParser.Parsers;

public static class SkinParser
{
    public static List<SkinEntry> Parse(string filePath)
    {
        using var reader = new BigEndianBinaryReader(File.OpenRead(filePath));
        var count = reader.ReadUInt16();
        var result = new List<SkinEntry>(count);

        for (var i = 0; i < count; i++)
        {
            var id = reader.ReadUInt32();
            reader.ReadLengthPrefixedString(); // identifier
            var name = reader.ReadLengthPrefixedString();
            reader.ReadLengthPrefixedString(); // filename
            reader.ReadByte(); // unknown
            reader.ReadLengthPrefixedString(); // tooltip_jp
            reader.ReadLengthPrefixedString(); // tooltip_en

            result.Add(new SkinEntry { Id = id, SkinName = name });
        }

        return result;
    }
}

public class SkinEntry
{
    [JsonPropertyName("Id")]
    public uint Id { get; set; }

    [JsonPropertyName("SkinName")]
    public string SkinName { get; set; } = string.Empty;
}
