using System.ComponentModel.DataAnnotations;

namespace TNTCalculatorRazor.Domain.Enums;

public enum DiseaseType
{
    [Display(Name = "なし")]
    None,
    [Display(Name = "糖尿病")] 
    Diabetes,           // 糖尿病
    [Display(Name = "腎疾患未透析")] 
    RenalFailure,       // 腎疾患（未透析）
    [Display(Name = "血液透析")] 
    Hemodialysis,       // 血液透析
    [Display(Name = "肝硬変")] 
    LiverCirrhosis      // 肝硬変
}