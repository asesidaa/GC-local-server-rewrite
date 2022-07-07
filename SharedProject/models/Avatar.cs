using ProtoBuf;

namespace SharedProject.models;

[ProtoContract]
public class Avatar
{
    [ProtoMember(1)]
    public uint Id { get; set; }
    
    [ProtoMember(2)]
    public string? IdString { get; set; }
    
    [ProtoMember(3)]
    public string? FullName { get; set; }
    
    [ProtoMember(4)]
    public string? Name { get; set; }
    
    [ProtoMember(5)]
    public string? Variant { get; set; }
    
    [ProtoMember(6)]
    public string? AcquireMethodJp { get; set; }
    
    [ProtoMember(7)]
    public string? AcquireMethodEn { get; set; }
    
    public override string ToString() {
        return $"{Id}: {FullName}, {AcquireMethodEn}";
    }
}