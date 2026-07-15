using System.Text.Json.Serialization;

namespace DatParser.Parsers;

public static class TitleParser
{
    public static List<TitleEntry> Parse(string filePath)
    {
        using var reader = new BigEndianBinaryReader(File.OpenRead(filePath));
        var count = reader.ReadUInt16();
        var result = new List<TitleEntry>(count);

        for (var i = 0; i < count; i++)
        {
            var id = reader.ReadUInt32();
            reader.ReadLengthPrefixedString(); // id_str
            reader.ReadLengthPrefixedString(); // file_suffix
            var nameJp = reader.ReadLengthPrefixedString();
            reader.ReadLengthPrefixedString(); // name_en
            reader.ReadByte(); // enabled
            var unlockReqJp = reader.ReadLengthPrefixedString();
            var unlockReqEn = reader.ReadLengthPrefixedString();
            reader.ReadLengthPrefixedString(); // next_unlock_requirement_jp
            reader.ReadLengthPrefixedString(); // next_unlock_requirement_en
            var titleType = reader.ReadByte();
            reader.ReadByte(); // unknown0
            reader.ReadByte(); // unknown1
            reader.ReadUInt16(); // related_song_id (BE u2)
            reader.ReadByte(); // title_chain_id

            result.Add(new TitleEntry
            {
                Id = id,
                TitleName = nameJp,
                UnlockRequirementJp = unlockReqJp,
                UnlockRequirementEn = unlockReqEn,
                UnlockType = titleType
            });
        }

        return result;
    }
}

public class TitleEntry
{
    [JsonPropertyName("Id")]
    public uint Id { get; set; }

    [JsonPropertyName("TitleName")]
    public string TitleName { get; set; } = string.Empty;

    [JsonPropertyName("UnlockRequirementJp")]
    public string UnlockRequirementJp { get; set; } = string.Empty;

    [JsonPropertyName("UnlockRequirementEn")]
    public string UnlockRequirementEn { get; set; } = string.Empty;

    [JsonPropertyName("UnlockType")]
    public int UnlockType { get; set; }
}
