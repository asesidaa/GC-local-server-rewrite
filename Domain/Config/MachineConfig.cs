namespace Domain.Config;

public class MachineConfig
{
    public const string MACHINE_SECTION = "Config";

     public bool MachineCheckEnable { get; set; }
     public List<Machine> Machines { get; set; } = new();
}

public class Machine
{
    public string Name { get; set; } = string.Empty;
    public string Pref { get; set; } = string.Empty;
    public string TenpoID { get; set; } = string.Empty;
    public string Mac { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;


}