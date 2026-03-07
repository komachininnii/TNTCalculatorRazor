using TNTCalculatorRazor.Domain.Enums;
using TNTCalculatorRazor.Domain.Models;

namespace TNTCalculatorRazor.Domain.Tables;

public static class EnteralFormulaTable
{
    public static EnteralFormulaComposition Get( EnteralFormulaType type )
        => EnteralFormulaData.GetComposition(type);
}
