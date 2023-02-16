using System.Text.Json.Serialization;
using Domain.Enums;

namespace Domain.Config;

public class UnlockRewardConfig
{
    public int RewardId { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RewardType RewardType { get; set; }

    public int TargetId { get; set; }

    public int TargetNum { get; set; }

    public int KeyNum { get; set; }
}