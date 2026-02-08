using System.ComponentModel.DataAnnotations;

namespace TNTCalculatorRazor.Domain.Enums;

public enum BmrWeightBasisType
{
    //[Display(Name = "実測")]
    Actual,

    //[Display(Name = "標準")]
    Standard,

    //[Display(Name = "調整")]
    Adjusted
}
