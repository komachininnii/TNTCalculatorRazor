using System;
using System.Collections.Generic;
using System.Text;
using TNTCalculatorRazor.Domain.Calculators;
using TNTCalculatorRazor.Domain.Enums;
using Xunit;

namespace TNTCalculatorRazor.Tests.Domain.Calculators;

public class CorrectedWeightCalculatorTests
{
    [Theory]
    // 乳児(0歳)は常に実測
    [InlineData(0, 9.0, 8.0, 50.0, 9.0)]
    // 80%以下 → 標準
    [InlineData(30, 60.0, 50.0, 80.0, 50.0)]
    [InlineData(30, 60.0, 50.0, 79.9, 50.0)]
    // 120%以上 → 調整
    // adjusted = (actual-standard)*0.25 + standard
    [InlineData(30, 80.0, 60.0, 120.0, 65.0)]
    [InlineData(30, 80.0, 60.0, 150.0, 65.0)]
    // 80超〜120未満 → 実測
    [InlineData(30, 60.0, 50.0, 80.1, 60.0)]
    [InlineData(30, 60.0, 50.0, 119.9, 60.0)]
    public void CalculateCorrectedWeight_境界を含めて正しく採用体重が決まる(
        int age, double actual, double standard, double obesity, double expected )
    {
        var corrected = CorrectedWeightCalculator.CalculateCorrectedWeight(
            age, actual, standard, obesity);

        Assert.Equal(expected, corrected, 6);
    }

    [Theory]
    [InlineData(80.0, 60.0, 65.0)] // (80-60)*0.25 + 60 = 65
    [InlineData(50.0, 40.0, 42.5)] // (50-40)*0.25 + 40 = 42.5
    public void CalculateAdjustedWeight_計算式が正しい( double actual, double standard, double expected )
    {
        var adjusted = CorrectedWeightCalculator.CalculateAdjustedWeight(actual, standard);
        Assert.Equal(expected, adjusted, 6);
    }

    [Theory]
    [InlineData(0, 10.0, BmrWeightBasisType.Actual)]
    [InlineData(30, 80.0, BmrWeightBasisType.Standard)]
    [InlineData(30, 79.9, BmrWeightBasisType.Standard)]
    [InlineData(30, 120.0, BmrWeightBasisType.Adjusted)]
    [InlineData(30, 150.0, BmrWeightBasisType.Adjusted)]
    [InlineData(30, 100.0, BmrWeightBasisType.Actual)]
    public void GetBasis_境界を含めて正しい( int age, double obesity, BmrWeightBasisType expected )
    {
        var basis = CorrectedWeightCalculator.GetBasis(age, obesity);
        Assert.Equal(expected, basis);
    }
}
