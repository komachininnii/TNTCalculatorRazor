using TNTCalculatorRazor.Domain.Enums;

namespace TNTCalculatorRazor.Domain.Tables;

public static class EnteralPackageTable
{
    public static IReadOnlyList<int> Get( EnteralFormulaType type )
        => EnteralFormulaData.GetPackages(type);
}
