using System.Collections.Generic;

namespace TNTCalculatorRazor.Domain.Models;

public sealed class EnteralFormulaInfo
{
    public EnteralFormulaComposition Composition { get; }
    public IReadOnlyList<int> Packages { get; }

    public EnteralFormulaInfo( EnteralFormulaComposition composition, IReadOnlyList<int> packages )
    {
        Composition = composition;
        Packages = packages;
    }
}
