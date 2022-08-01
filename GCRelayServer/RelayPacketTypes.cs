namespace GCRelayServer;

public static class RelayPacketTypes
{
    public const ushort HEART_BEAT = 0xB0;
    
    public const ushort HEART_BEAT_RESPONSE = 0xB1;
    
    public const ushort START_MATCHING = 0xA0;
    
    public const ushort START_MATCHING_RESPONSE = 0xA1;
    
    public const ushort REGISTER_MATCHING = 0xA6;
}