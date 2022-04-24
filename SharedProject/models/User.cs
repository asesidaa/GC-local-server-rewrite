namespace SharedProject.models;

public class User
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public long CardId { get; set; }

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public string PlayerName { get; set; } = string.Empty;
}