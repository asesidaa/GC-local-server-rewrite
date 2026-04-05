using System.Text.Json.Serialization;

namespace DatParser.Parsers;

public static class NavigatorParser
{
    public static List<NavigatorEntry> Parse(string filePath)
    {
        using var reader = new BigEndianBinaryReader(File.OpenRead(filePath));
        var count = reader.ReadUInt16();
        var result = new List<NavigatorEntry>(count);

        for (var i = 0; i < count; i++)
        {
            var id = reader.ReadUInt32();
            reader.ReadLengthPrefixedString(); // identifier
            reader.ReadLengthPrefixedString(); // file_name

            // name_entries0 (JP): name_with_variant, name, variant, illustration_credit
            reader.ReadLengthPrefixedString();
            reader.ReadLengthPrefixedString();
            reader.ReadLengthPrefixedString();
            reader.ReadLengthPrefixedString();

            // name_entries1 (EN): name_with_variant, name, variant, illustration_credit
            var enNameWithVariant = reader.ReadLengthPrefixedString();
            reader.ReadLengthPrefixedString(); // name
            reader.ReadLengthPrefixedString(); // variant
            var illustrationCredit = reader.ReadLengthPrefixedString();

            var genre = reader.ReadByte();
            reader.ReadByte(); // unknown_enum0
            reader.ReadByte(); // unknown_bool0
            reader.ReadByte(); // unknown_bool1
            reader.ReadByte(); // unknown_bool2
            reader.Skip(7);    // zeros
            reader.ReadByte(); // default_availability
            reader.ReadByte(); // unknown_enum1
            reader.ReadByte(); // unknown_bool3
            var tooltipJp = reader.ReadLengthPrefixedString();
            var tooltipEn = reader.ReadLengthPrefixedString();

            result.Add(new NavigatorEntry
            {
                Id = id,
                NavigatorName = enNameWithVariant,
                Genre = genre,
                IllustrationCredit = illustrationCredit,
                ToolTipJp = tooltipJp,
                ToolTipEn = tooltipEn
            });
        }

        return result;
    }
}

public class NavigatorEntry
{
    [JsonPropertyName("Id")]
    public uint Id { get; set; }

    [JsonPropertyName("NavigatorName")]
    public string NavigatorName { get; set; } = string.Empty;

    [JsonPropertyName("Genre")]
    public int Genre { get; set; }

    [JsonPropertyName("IllustrationCredit")]
    public string IllustrationCredit { get; set; } = string.Empty;

    [JsonPropertyName("ToolTipJp")]
    public string ToolTipJp { get; set; } = string.Empty;

    [JsonPropertyName("ToolTipEn")]
    public string ToolTipEn { get; set; } = string.Empty;
}
