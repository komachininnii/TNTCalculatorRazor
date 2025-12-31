using System.ComponentModel.DataAnnotations;

namespace TNTCalculatorRazor.Domain.Enums;

public enum PressureUlcerLevel
{
    [Display(Name = "なし")]
    None = 0,

    [Display(Name = "d1〜d2（ストレス係数+0.1）")]
    D1ToD2 = 1,

    [Display(Name = "D3（ストレス係数+0.2）")]
    D3 = 2,

    [Display(Name = "D4（ストレス係数+0.3）")]
    D4 = 3,

    [Display(Name = "D5（ストレス係数+0.4）")]
    D5 = 4
}