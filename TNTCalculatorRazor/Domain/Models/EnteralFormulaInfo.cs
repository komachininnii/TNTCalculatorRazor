using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TNTCalculatorRazor.Domain.Models;

public sealed class EnteralFormulaInfo
{
    public EnteralFormulaComposition Composition { get; }
    public IReadOnlyList<int> Packages { get; }

    public EnteralFormulaInfo( EnteralFormulaComposition composition, IReadOnlyList<int> packages )
    {
        Composition = composition;
        Packages = new ReadOnlyCollection<int>(packages.ToArray());
    }
}
