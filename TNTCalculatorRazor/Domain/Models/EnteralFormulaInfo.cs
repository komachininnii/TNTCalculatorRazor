using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TNTCalculatorRazor.Domain.Models;

public sealed class EnteralFormulaInfo
{
    public string DisplayName { get; }
    public EnteralFormulaComposition Composition { get; }
    public IReadOnlyList<int> Packages { get; }

    public EnteralFormulaInfo(
        string displayName,
        EnteralFormulaComposition composition,
        IReadOnlyList<int> packages )
    {
        DisplayName = string.IsNullOrWhiteSpace(displayName)
            ? throw new ArgumentException("displayName は必須です。", nameof(displayName))
            : displayName;
        Composition = composition;
        Packages = new ReadOnlyCollection<int>(packages.ToArray());
    }
}
