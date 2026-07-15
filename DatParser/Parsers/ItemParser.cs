using System.Text.Json.Serialization;

namespace DatParser.Parsers;

public static class ItemParser
{
    public static List<ItemEntry> Parse(string filePath)
    {
        using var reader = new BigEndianBinaryReader(File.OpenRead(filePath));
        var count = reader.ReadUInt16();
        var result = new List<ItemEntry>(count);

        for (var i = 0; i < count; i++)
        {
            var id = reader.ReadUInt32();
            reader.ReadLengthPrefixedString(); // id_str
            var name = reader.ReadLengthPrefixedString();
            reader.ReadByte(); // unknown_bool
            reader.ReadLengthPrefixedString(); // item_tooltip_jp
            reader.ReadLengthPrefixedString(); // item_tooltip_en

            result.Add(new ItemEntry { Id = id, ItemName = name });
        }

        return result;
    }
}

public class ItemEntry
{
    [JsonPropertyName("Id")]
    public uint Id { get; set; }

    [JsonPropertyName("ItemName")]
    public string ItemName { get; set; } = string.Empty;
}
