using System.ComponentModel.DataAnnotations;

namespace TNTCalculatorRazor.Domain.Enums;

public enum EnergyOrderType
{
    [Display(Name = "推定必要カロリー")]
    BmrEstimated,   // BMR推定
    [Display(Name = "25kcal/標準体重")]
    Kcal25,         // 25 kcal / 標準体重
    [Display(Name = "30kcal/標準体重")]
    Kcal30,         // 30 kcal / 標準体重
    [Display(Name = "35kcal/標準体重")]
    Kcal35,         // 35 kcal / 標準体重
    [Display(Name = "手入力")]
    Manual          // 手入力
}