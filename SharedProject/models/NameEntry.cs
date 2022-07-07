using ProtoBuf;

namespace SharedProject.models;

[ProtoContract]
public class NameEntry
{
    [ProtoMember(1)]
    public string? NameWithVariant { get; set; }
    [ProtoMember(2)]
    public string? NameWithoutVariant{ get; set; }
    [ProtoMember(3)]
    public string? Variant{ get; set; }
    [ProtoMember(4)]
    public string? IllustrationCredit{ get; set; }
    public override string ToString()
    {
        return $"{NameWithVariant}, {IllustrationCredit}";
    }
}