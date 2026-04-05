namespace Domain.Config;

public class WebAuthConfig
{
    public const string WEB_AUTH_SECTION = "WebAuth";

    public string AdminPassword { get; set; } = string.Empty;
}
