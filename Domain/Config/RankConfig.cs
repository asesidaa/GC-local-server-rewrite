using System.ComponentModel.DataAnnotations;

namespace Domain.Config;

public class RankConfig
{
    public const string RANK_SECTION = "Rank";
    
    [Required, Range(1, double.MaxValue, ErrorMessage = "Value for {0} must be greater than 0!")]
    public int RefreshIntervalHours { get; set; } = 24;
}