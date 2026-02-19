using System;
using System.Collections.Generic;
using System.Text;
using TNTCalculatorRazor.Domain.Enums;

namespace TNTCalculatorRazor.Tests.TestData;

public static class EnteralFormulaTestCases
{
    // 経腸栄養剤のテストケース
    public static IEnumerable<object[]> CurrentFormulas()
    {
        yield return new object[] { EnteralFormulaType.Meibalance10 };
        yield return new object[] { EnteralFormulaType.PeptamenPrebio15 };
        yield return new object[] { EnteralFormulaType.PeptamenIntense10 };
        yield return new object[] { EnteralFormulaType.PeptamenAF15 };
        yield return new object[] { EnteralFormulaType.IsocalSupport15 };
        yield return new object[] { EnteralFormulaType.Lacphia15 };
        yield return new object[] { EnteralFormulaType.Mein10 };
        yield return new object[] { EnteralFormulaType.RenalenMP16 };
        yield return new object[] { EnteralFormulaType.GlucernaRex10 };
        yield return new object[] { EnteralFormulaType.PGSoftEJ15 };
        yield return new object[] { EnteralFormulaType.RacolNF10 };
        yield return new object[] { EnteralFormulaType.RacolNFSemiSolid10 };
        yield return new object[] { EnteralFormulaType.EnsureH15 };
        yield return new object[] { EnteralFormulaType.Inoras16 };
        yield return new object[] { EnteralFormulaType.Elental10 };
    }
}
