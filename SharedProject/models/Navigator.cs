using ProtoBuf;
using SharedProject.enums;

namespace SharedProject.models;

[ProtoContract]
public class Navigator
{
    [ProtoMember(1)]
    public uint Id { get; set; }

    [ProtoMember(2)]
    public string? IdString { get; set; }

    [ProtoMember(3)]
    public string? FileName { get; set; }
    
    [ProtoMember(4)]
    public NameEntry? NameEntry0 { get; set; }
    
    [ProtoMember(5)]
    public NameEntry? NameEntry1 { get; set; }

    [ProtoMember(6)]
    public NavigatorGenre Genre { get; set; }
    
    [ProtoMember(7)]
    public NavigatorDefaultAvailability DefaultAvailability { get; set; }
    
    [ProtoMember(8)]
    public string? ToolTipJp { get; set; }
    
    [ProtoMember(9)]
    public string? ToolTipEn { get; set; }

    [ProtoIgnore]
    public string? NameEntryString => NameEntry0?.ToString();

    public override string ToString() {
        return $"{Id}: {NameEntry1?.NameWithVariant}, {NameEntry1?.IllustrationCredit}";
    }
    
    
}