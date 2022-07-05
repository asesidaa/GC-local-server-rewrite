using System.Text.Json.Serialization;
using SharedProject.enums;

namespace SharedProject.models;

public class PlayOption
{
    [JsonPropertyName(nameof(CardId))]
    public long CardId { get; set; }

    [JsonPropertyName(nameof(FastSlowIndicator))]
    public PlayOptions.FastSlowIndicator FastSlowIndicator { get; set; }

    [JsonPropertyName(nameof(FeverTrance))]
    public PlayOptions.FeverTranceShow FeverTrance { get; set; }
    
    [JsonPropertyName(nameof(AvatarId))]
    public long AvatarId { get; set; }
    
    [JsonPropertyName(nameof(NavigatorId))]
    public long NavigatorId { get; set; }
    
    [JsonPropertyName(nameof(TitleId))]
    public long TitleId { get; set; }
}