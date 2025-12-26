using System.ComponentModel.DataAnnotations;

namespace TNTCalculatorRazor.Domain.Enums;

public enum StressFactorType
{
    [Display(Name = "正常(1.0)")]
    Normal,              // 1.0
    [Display(Name = "軽度障害(1.1)")]
    MildStress,          // 1.1

    [Display(Name = "感染症：軽症(1.2)")]
    InfectMild,          // 1.2
    [Display(Name = "感染症：中等度(1.5)")]
    InfectModerate,      // 1.5
    [Display(Name = "感染症：重症(1.8)")]
    InfectSevere,        // 1.8

    [Display(Name = "手術：胆嚢切除・乳房切除(1.2)")]
    SurgeryMinor,        // 1.2
    [Display(Name = "手術：胃亜全摘・大腸切除(1.4)")]
    SurgeryModerate,     // 1.4
    [Display(Name = "手術：胃全摘・胆管切除(1.6)")]
    SurgeryMajor,        // 1.6
    [Display(Name = "手術：膵頭切除・肝切除・食道切除(1.8)")]
    SurgeryVeryMajor,    // 1.8

    [Display(Name = "外傷：骨折(1.35)")]
    TraumaBone,          // 1.35
    [Display(Name = "外傷：筋肉外傷(1.3)")]
    TraumaMuscle,        // 1.3
    [Display(Name = "外傷：頭部外傷＋ステロイド(1.6)")]
    TraumaHeadSteroid,   // 1.6

    [Display(Name = "熱傷：20%(1.5)")]
    Burn20,              // 1.5
    [Display(Name = "熱傷：40%(1.85)")]
    Burn40,              // 1.85
    [Display(Name = "熱傷：100%(2.05)")]
    Burn100              // 2.05
}
