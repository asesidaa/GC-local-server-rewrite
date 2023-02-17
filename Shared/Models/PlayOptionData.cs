using Shared.Dto.Api;

namespace Shared.Models;

public class PlayOptionData
{
    public long CardId { get; set; }
    
    public FirstPlayOptionDto OptionPart1 { get; set; } = new();

    public SecondPlayOptionDto OptionPart2 { get; set; } = new();
}