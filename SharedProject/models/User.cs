using System.Text.Json.Serialization;

namespace SharedProject.models;

public class User
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    [JsonPropertyName(nameof(CardId))]
    public long CardId { get; set; }

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    [JsonPropertyName(nameof(PlayerName))]
    public string PlayerName { get; set; } = string.Empty;
}