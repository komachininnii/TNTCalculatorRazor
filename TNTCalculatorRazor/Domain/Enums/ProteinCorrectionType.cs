using System.ComponentModel.DataAnnotations;

namespace TNTCalculatorRazor.Domain.Enums;

public enum ProteinCorrectionType
{
    [Display(Name = "なし")]
    None,
    [Display(Name = "CKD 3b〜5 (0.7)")]
    CKD3bTo5,           // 0.7
    [Display(Name = "蛋白不耐(0.5)")] 
    LiverCirrhosisPoor  // 0.5
}