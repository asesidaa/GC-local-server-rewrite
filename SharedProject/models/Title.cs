using SharedProject.enums;

namespace SharedProject.models;

public class Title
{
    public uint Id { get; set; }
    public string? IdString { get; set; }
    public string? NameJp { get; set; }
    public string? NameEng { get; set; }
    public string? UnlockRequirementJp { get; set; }
    public string? UnlockRequirementEng { get; set; }
    public TitleUnlockType Type { get; set; }
    public override string ToString() {
        return $"{Id}: {NameEng}, {UnlockRequirementEng}";
    }
}