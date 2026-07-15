namespace Shared.Dto.Api;

public class DefaultUnlockDetailDto
{
    public string ItemType { get; set; } = string.Empty;

    public int TotalCount { get; set; }

    public long[] Bitset { get; set; } = [];
}
