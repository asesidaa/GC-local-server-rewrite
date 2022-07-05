namespace SharedProject.models;

public class Avatar
{
    public uint Id { get; set; }
    public string? IdString { get; set; }
    public string? FullName { get; set; }
    public string? Name { get; set; }
    public string? Variant { get; set; }
    public string? AcquireMethod { get; set; }
    
    public override string ToString() {
        return $"{Id}: {FullName}, {AcquireMethod}";
    }
}