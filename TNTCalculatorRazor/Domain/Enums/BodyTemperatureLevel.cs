using System.ComponentModel.DataAnnotations;

namespace TNTCalculatorRazor.Domain.Enums;

public enum BodyTemperatureLevel
{
    [Display(Name = "平熱")]
    Normal = 0,

    [Display(Name = "37℃台(ストレス係数+0.2)")]
    Fever37 = 1,

    [Display(Name = "38℃台(ストレス係数+0.4)")]
    Fever38 = 2,

    [Display(Name = "39℃台(ストレス係数+0.6)")]
    Fever39 = 3,

    [Display(Name = "40℃以上(ストレス係数+0.8)")]
    Fever40 = 4
}