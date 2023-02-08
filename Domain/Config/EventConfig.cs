namespace Domain.Config;

public class EventConfig
{
    public const string EVENT_SECTION = "Events";

    public bool UseEvents { get; set; }

    public List<EventFile> EventFiles { get; set; } = new();
}

public class EventFile
{
    public string FileName { get; set; } = string.Empty;

    public int Index { get; set; }
}