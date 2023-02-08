namespace Domain.Models;

public class Event
{
    public string Name { get; set; } = string.Empty;

    public string Md5 { get; set; } = string.Empty;

    public int Index { get; set; }

    public string NotBefore { get; set; } = string.Empty;

    public string NotAfter { get; set; } = string.Empty;
}