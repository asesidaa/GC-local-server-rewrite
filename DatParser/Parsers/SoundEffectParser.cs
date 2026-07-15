using System.Text.Json.Serialization;

namespace DatParser.Parsers;

public static class SoundEffectParser
{
    public static List<SoundEffectEntry> Parse(string filePath)
    {
        using var reader = new BigEndianBinaryReader(File.OpenRead(filePath));
        var count = reader.ReadUInt16();
        var result = new List<SoundEffectEntry>(count);

        for (var i = 0; i < count; i++)
        {
            var id = reader.ReadUInt32();
            reader.ReadLengthPrefixedString(); // identifier
            var fullName = reader.ReadLengthPrefixedString();
            reader.ReadLengthPrefixedString(); // name_line1
            reader.ReadLengthPrefixedString(); // name_line2/variant
            reader.ReadLengthPrefixedString(); // arrange_filename
            reader.ReadLengthPrefixedString(); // tap_se1_filename
            reader.ReadLengthPrefixedString(); // tap_se2_filename
            reader.ReadByte(); // unknown
            reader.ReadLengthPrefixedString(); // tooltip_jp
            reader.ReadLengthPrefixedString(); // tooltip_en

            result.Add(new SoundEffectEntry { Id = id, SoundEffectName = fullName });
        }

        return result;
    }
}

public class SoundEffectEntry
{
    [JsonPropertyName("Id")]
    public uint Id { get; set; }

    [JsonPropertyName("SoundEffectName")]
    public string SoundEffectName { get; set; } = string.Empty;
}
