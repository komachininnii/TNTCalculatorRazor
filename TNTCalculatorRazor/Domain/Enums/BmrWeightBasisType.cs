using System.ComponentModel.DataAnnotations;

namespace TNTCalculatorRazor.Domain.Enums;

public enum BmrWeightBasisType
{
    //[Display(Name = "実")]
    Actual,

    //[Display(Name = "標")]
    Standard,

    //[Display(Name = "調")]
    Adjusted
}
