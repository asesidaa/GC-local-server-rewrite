namespace SharedProject.models;

public class NameEntry
{
    public string? NameWithVariant { get; set; }
    public string? NameWithoutVariant{ get; set; }
    public string? Variant{ get; set; }
    public string? IllustrationCredit{ get; set; }
    public override string ToString() {
        return $"{NameWithVariant}";
    }
}