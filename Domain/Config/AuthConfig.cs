namespace Domain.Config;

public class AuthConfig
{
    public const string AUTH_SECTION = "Auth";
    
    public bool Enabled { get; set; }
    
    public List<Machine> Machines { get; set; } = new();
}

public class Machine
{
    public string TenpoId { get; set; } = string.Empty;
    public string TenpoName { get; set; } = string.Empty;
    public string Pref { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Mac { get; set; } = string.Empty;
}