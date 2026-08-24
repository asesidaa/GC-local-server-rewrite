namespace GCRelayServer.Protocol;

public enum RelayMessageType : ushort
{
    PhaseBarrier = 0x0010,
    MusicSelection = 0x0030,
    MusicSelectionCommit = 0x0031,
    DifficultyAndItem = 0x0032,
    LiveTuneSnapshot = 0x0040,
    ResultRecord = 0x0050,
    PresetMessage = 0x0060,
    GuestLiveness = 0x0070,
    HostStatus = 0x0071,
    Registration = 0x00A0,
    RegistrationAck = 0x00A1,
    NewMember = 0x00A2,
    Leave = 0x00A3,
    Announcement = 0x00A6,
    Start = 0x00A7,
    HeartbeatRequest = 0x00B0,
    HeartbeatResponse = 0x00B1
}
