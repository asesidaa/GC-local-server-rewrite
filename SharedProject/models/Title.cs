using ProtoBuf;
using SharedProject.enums;

namespace SharedProject.models;

[ProtoContract]
public class Title
{
    [ProtoMember(1)]
    public uint Id { get; set; }
    
    [ProtoMember(2)]
    public string? IdString { get; set; }
    
    [ProtoMember(3)]
    public string? NameJp { get; set; }
    
    [ProtoMember(4)]
    public string? NameEng { get; set; }
    
    [ProtoMember(5)]
    public string? UnlockRequirementJp { get; set; }
    
    [ProtoMember(6)]
    public string? UnlockRequirementEng { get; set; }
    
    [ProtoMember(7)]
    public TitleUnlockType Type { get; set; }
    public override string ToString() {
        return $"{Id}: {NameEng}, {UnlockRequirementEng}";
    }
}