using System;
using System.Collections.Generic;
using System.Text;
using TNTCalculatorRazor.Domain.Calculators;
using Xunit;

namespace TNTCalculatorRazor.Tests.Domain.Calculators;

public sealed class BodySurfaceAreaCalculatorTests
{
    [Fact]
    public void Calculate_代表値_170cm60kgは概ね妥当範囲()
    {
        // DuBois系の代表点：170cm/60kg でだいたい 1.7 m^2 前後
        var bsa = BodySurfaceAreaCalculator.Calculate(170.0, 60.0);

        Assert.InRange(bsa, 1.60, 1.90);
    }

    [Fact]
    public void Calculate_身長体重が増えればBSAも増える()
    {
        var small = BodySurfaceAreaCalculator.Calculate(160.0, 50.0);
        var large = BodySurfaceAreaCalculator.Calculate(170.0, 60.0);

        Assert.True(large > small);
    }

    [Theory]
    [InlineData(170.0, 0.0)]
    [InlineData(0.0, 60.0)]
    [InlineData(-170.0, 60.0)]
    [InlineData(170.0, -60.0)]
    public void Calculate_入力が0以下を含むと有限値にならない挙動を固定( double h, double w )
    {
        var bsa = BodySurfaceAreaCalculator.Calculate(h, w);

        // 現実的には height/weight <=0 は不正入力。
        // 現実装は例外ではなく NaN/0/Infinity になり得るので、
        // 「有限な正の値にはならない」を不変条件として固定しておく。
        Assert.True(
            double.IsNaN(bsa) || double.IsInfinity(bsa) || bsa <= 0.0,
            $"Expected non-finite or non-positive for invalid inputs. bsa={bsa}, h={h}, w={w}");
    }
}
