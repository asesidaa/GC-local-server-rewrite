using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Domain.Enums;

[EnumExtensions]
public enum ShowFeverTranceOption : long
{
    [Display(Name = "Default option")]
    Default  = 0,
    
    [Display(Name = "Show fever/trance")]
    Show     = 1,
    
    [Display(Name = "Do not show fever/trance")]
    NotShow  = 2
}