using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using TNTCalculatorRazor.Domain.Enums;

namespace TNTCalculatorRazor.Tests.TestData;

public static class EnteralFormulaTestCases
{
    // 経腸栄養剤のテストケース
    public static IEnumerable<object[]> CurrentFormulas_WithPackKcalAndVolume()
    {
        yield return new object[] { EnteralFormulaType.Meibalance10, 400.0, 400.0 };        // 400kcal / 400mL
        yield return new object[] { EnteralFormulaType.PeptamenPrebio15, 400.0, 267.0 };    // 400kcal / 267mL
        yield return new object[] { EnteralFormulaType.PeptamenIntense10, 200.0, 200.0 };   // 200kcal / 200mL
        yield return new object[] { EnteralFormulaType.PeptamenAF15, 300.0, 200.0 };        // 300kcal / 200mL
        yield return new object[] { EnteralFormulaType.IsocalSupport15, 400.0, 267.0 };     // 400kcal / 267mL
        yield return new object[] { EnteralFormulaType.Lacphia15, 400.0, 267.0 };
        yield return new object[] { EnteralFormulaType.Mein10, 200.0, 200.0 }; 
        yield return new object[] { EnteralFormulaType.RenalenMP16, 400.0, 250.0 };
        yield return new object[] { EnteralFormulaType.GlucernaRex10, 400.0, 400.0 };
        yield return new object[] { EnteralFormulaType.PGSoftEJ15, 400.0, 267.0 };
        yield return new object[] { EnteralFormulaType.RacolNF10, 200.0, 200.0 };
        yield return new object[] { EnteralFormulaType.RacolNFSemiSolid10, 300.0, 300.0 };
        yield return new object[] { EnteralFormulaType.EnsureH15, 375.0, 250.0 };
        yield return new object[] { EnteralFormulaType.Inoras16, 300.0, 187.5 };
        yield return new object[] { EnteralFormulaType.Elental10, 300.0, 300.0 };
    }

    // typeだけ欲しいテスト向けに、上から生成する
    public static IEnumerable<object[]> CurrentFormulas()
        => CurrentFormulas_WithPackKcalAndVolume()
            .Select(x => new object[] { (EnteralFormulaType)x[0] });

}
