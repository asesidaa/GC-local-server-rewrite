namespace Shared.Dto.Api;

public class MusicFavoriteDto
{
    public long CardId { get; set; }

    public int MusicId { get; set; }

    public bool IsFavorite { get; set; }
}