using System.Text.Json.Serialization;

namespace SharedProject.models;

public class MusicFavoriteData
{
    [JsonPropertyName(nameof(CardId))]
    public long CardId { get; set; }
    
    [JsonPropertyName(nameof(MusicId))]
    public int MusicId { get; set; }
    
    [JsonPropertyName(nameof(IsFavorite))]
    public bool IsFavorite { get; set; }
}