using System;
using System.Collections.Generic;
using System.Text;
using TNTCalculatorRazor.Domain.Calculators;
using TNTCalculatorRazor.Domain.Enums;
using TNTCalculatorRazor.Domain.Rules;
using Xunit;

namespace TNTCalculatorRazor.Tests.Domain.Calculators;

public class CcrCalculator_WithCorrection_Tests
{
    [Fact]
    public void 年齢70歳以上男性_Crが0_8未満なら補正Crで計算する想定の差を確認()
    {
        int age = 80;
        double w = 60.0;
        double crInput = 0.6;

        // 補正なし
        var ccrRaw = CcrCalculator.Calculate(age, w, crInput, GenderType.Male);

        // 補正あり（0.8）
        var correctedCr = CcrCreatinineCorrectionRule.GetCorrectedCreatinine(age, GenderType.Male, crInput);
        Assert.Equal(0.8, correctedCr);

        var ccrCorrected = CcrCalculator.Calculate(age, w, correctedCr!.Value, GenderType.Male);

        Assert.True(ccrCorrected < ccrRaw); // Crを大きく補正するのでCCrは下がる
    }
}
