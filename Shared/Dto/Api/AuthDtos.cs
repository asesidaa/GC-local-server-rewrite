namespace Shared.Dto.Api;

public class AdminLoginRequest
{
    public string Password { get; set; } = string.Empty;
}

public class CardLoginRequest
{
    public long CardId { get; set; }

    public string AccessCode { get; set; } = string.Empty;
}

public class SetAccessCodeRequest
{
    public long CardId { get; set; }

    public string AccessCode { get; set; } = string.Empty;
}

public class AuthStatusDto
{
    public bool IsAuthenticated { get; set; }

    public string? Role { get; set; }

    public long? CardId { get; set; }
}
