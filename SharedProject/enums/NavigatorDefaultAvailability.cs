using ProtoBuf;

namespace SharedProject.enums;

[ProtoContract]
public enum NavigatorDefaultAvailability
{
    NotAvailable = 0,
    Available = 1,
    AvailableWithVoice = 2,
}