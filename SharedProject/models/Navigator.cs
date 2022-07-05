using SharedProject.enums;

namespace SharedProject.models;

public class Navigator
{
    public uint Id { get; set; }

    public string? IdString { get; set; }

    public string? FileName { get; set; }
    
    public NameEntry? NameEntry0 { get; set; }
    public NameEntry? NameEntry1 { get; set; }

    public NavigatorGenre Genre { get; set; }
    
    public NavigatorDefaultAvailability DefaultAvailability { get; set; }
    
    public string? ToolTipJp { get; set; }
    public string? ToolTipEn { get; set; }
    public override string ToString() {
        return $"{Id}: {NameEntry1}, {ToolTipEn}";
    }
}