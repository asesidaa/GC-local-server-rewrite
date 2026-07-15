using System.Text.Json.Serialization;

namespace DatParser.Parsers;

public static class AvatarParser
{
    public static List<AvatarEntry> Parse(string filePath)
    {
        using var reader = new BigEndianBinaryReader(File.OpenRead(filePath));
        var count = reader.ReadUInt16();
        var result = new List<AvatarEntry>(count);

        for (var i = 0; i < count; i++)
        {
            var id = reader.ReadUInt32();
            reader.ReadLengthPrefixedString(); // id_str

            // name_entry_jp: full_name, name, variant
            reader.ReadLengthPrefixedString();
            reader.ReadLengthPrefixedString();
            reader.ReadLengthPrefixedString();

            // name_entry_en: full_name, name, variant
            var enFullName = reader.ReadLengthPrefixedString();
            reader.ReadLengthPrefixedString(); // name
            reader.ReadLengthPrefixedString(); // variant

            reader.ReadByte(); // unknown_byte1
            reader.ReadLengthPrefixedString(); // png_file_name
            reader.ReadLengthPrefixedString(); // uvb_file_name
            reader.ReadLengthPrefixedString(); // efcb2_file_name
            reader.ReadUInt32(); // unknown_int1
            reader.ReadByte(); // unknown_float1
            reader.ReadByte(); // unknown_float2
            reader.ReadByte(); // unknown_float3
            reader.ReadByte(); // unknown_float4
            reader.ReadByte(); // unknown_byte2
            reader.ReadUInt32(); // unknown_int2
            reader.ReadUInt32(); // unknown_int3
            reader.ReadUInt32(); // unknown_int4
            reader.ReadUInt32(); // unknown_int5
            reader.ReadByte(); // unknown_byte3
            reader.ReadLengthPrefixedString(); // acquire_method_jp
            reader.ReadLengthPrefixedString(); // acquire_method_en
            reader.ReadUInt32(); // unknown_int6

            result.Add(new AvatarEntry { AvatarId = id, AvatarName = enFullName });
        }

        return result;
    }
}

public class AvatarEntry
{
    [JsonPropertyName("AvatarId")]
    public uint AvatarId { get; set; }

    [JsonPropertyName("AvatarName")]
    public string AvatarName { get; set; } = string.Empty;
}
