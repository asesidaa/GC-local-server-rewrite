namespace GCRelayServer.Protocol;

public enum RelayRejectionCode
{
    None,
    DatagramTooShort,
    DatagramTooLarge,
    InvalidHeaderSize,
    InvalidRemainingSize,
    InvalidChannel,
    InvalidOuterReserved,
    InvalidPayloadSize,
    UnknownMessage,
    OutputOnlyMessage,
    InvalidMode,
    InvalidPayloadContract,
    NonZeroReservedPayload,
    InvalidMatchingId,
    InvalidEntryNumber,
    InvalidMachineId,
    InvalidTarget,
    RoomNotFound,
    MemberConflict,
    RoomFull,
    SenderNotRegistered,
    SenderEndpointMismatch,
    SenderRoleMismatch,
    TargetNotFound
}

public readonly record struct RelayRejection(RelayRejectionCode Code, string Detail)
{
    public static RelayRejection None => new(RelayRejectionCode.None, string.Empty);

    public override string ToString() => Code == RelayRejectionCode.None ? "None" : $"{Code}: {Detail}";
}
