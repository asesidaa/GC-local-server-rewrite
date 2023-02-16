namespace Domain.Enums;

public enum CardCommandType
{
    CardReadRequest  = 256,
    CardWriteRequest = 768,
    RegisterRequest  = 512,
    ReissueRequest   = 1536
}