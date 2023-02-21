using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Domain.Enums;

[EnumExtensions]
public enum ShowFastSlowOption : long
{
    [Display(Name = "Default option")]
    Default       = 0,
    
    [Display(Name = "Show fast/slow near avatars")]
    NearAvatar    = 1,
    
    [Display(Name = "Show fast/show near judgement text")]
    NearJudgement = 2,
    
    [Display(Name = "Do not show fast/slow")]
    NotShow       = 3
}