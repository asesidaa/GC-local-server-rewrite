namespace Shared.Dto.Api;

public class UnlockStateDto
{
    public string ItemType { get; set; } = string.Empty;

    public int TotalCount { get; set; }

    public int UnlockedCount { get; set; }
}
