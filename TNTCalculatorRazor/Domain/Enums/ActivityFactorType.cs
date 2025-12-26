using System.ComponentModel.DataAnnotations;

namespace TNTCalculatorRazor.Domain.Enums;

public enum ActivityFactorType
{
    [Display(Name = "臥床(1.0)")]
    BedriddenComa,        // 1.0

    [Display(Name = "覚醒(1.1)")]
    BedriddenAwake,       // 1.1

    [Display(Name = "起床(1.2)")]
    Sitting,              // 1.2

    [Display(Name = "離床(1.3)")]
    Wheelchair,           // 1.3

    [Display(Name = "歩行(1.4)")]
    Walking,              // 1.4

    [Display(Name = "リハビリ軽度(1.5)")]
    Rehabilitation15,     // 1.5

    [Display(Name = "リハビリ中等度(1.6)")]
    Rehabilitation16,     // 1.6

    [Display(Name = "リハビリ強度(1.7)")]
    Rehabilitation17      // 1.7
}