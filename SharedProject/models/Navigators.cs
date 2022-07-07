using ProtoBuf;

namespace SharedProject.models;

[ProtoContract]
public class Navigators
{
    [ProtoMember(1)]
    public int Count { get; set; }

    [ProtoMember(2)]
    public List<Navigator>? NavigatorList { get; set; }
}