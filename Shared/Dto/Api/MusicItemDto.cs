namespace Shared.Dto.Api;

public class MusicItemDto
{
    public long MusicId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Artist { get; set; }

    public bool UseFlag { get; set; }
}
