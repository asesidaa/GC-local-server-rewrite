namespace Domain.Config;

public class RelayConfig
{
    public const string RELAY_SECTION = "Relay";

    public string RelayServer { get; set; } = "127.0.0.1";

    public int RelayPort { get; set; } = 3333;
}