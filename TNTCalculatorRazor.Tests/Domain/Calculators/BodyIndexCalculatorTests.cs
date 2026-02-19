using System;
using System.Collections.Generic;
using System.Text;
using TNTCalculatorRazor.Domain.Calculators;
using TNTCalculatorRazor.Domain.Enums;
using Xunit;

namespace TNTCalculatorRazor.Tests.Domain.Calculators;

public sealed class BodyIndexCalculatorTests
{
    [Fact]
    public void Calculate_BMIが代表値通り()
    {
        // 170cm, 60kg → BMI = 60 / 1.7^2 = 20.761...
        var r = BodyIndexCalculator.Calculate(
            age: 30,
            heightCm: 170.0,
            weightKg: 60.0,
            gender: GenderType.Male);

        Assert.Equal(20.761, r.Bmi, precision: 3);
    }

    [Fact]
    public void Calculate_標準体重はStandardWeightCalculatorと一致()
    {
        var age = 10;
        var height = 135.0;
        var gender = GenderType.Female;

        var expected = StandardWeightCalculator.Calculate(age, height, gender);

        var r = BodyIndexCalculator.Calculate(age, height, weightKg: 30.0, gender);

        Assert.Equal(expected, r.StandardWeight, precision: 10);
    }

    [Fact]
    public void Calculate_ageが負なら例外()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            BodyIndexCalculator.Calculate(
                age: -1,
                heightCm: 170.0,
                weightKg: 60.0,
                gender: GenderType.Male));
    }

    [Fact]
    public void Calculate_age0は肥満度null()
    {
        var r = BodyIndexCalculator.Calculate(
            age: 0,
            heightCm: 70.0,
            weightKg: 8.0,
            gender: GenderType.Male);

        Assert.Null(r.ObesityDegree);
    }

    [Fact]
    public void Calculate_age1以上は肥満度が計算される()
    {
        var r = BodyIndexCalculator.Calculate(
            age: 1,
            heightCm: 80.0,
            weightKg: 10.0,
            gender: GenderType.Male);

        Assert.NotNull(r.ObesityDegree);
        Assert.True(r.ObesityDegree!.Value > 0);
    }

    [Fact]
    public void Calculate_体重が増えるとBMIも肥満度も増える()
    {
        var age = 10;
        var height = 135.0;
        var gender = GenderType.Female;

        var r1 = BodyIndexCalculator.Calculate(age, height, weightKg: 25.0, gender);
        var r2 = BodyIndexCalculator.Calculate(age, height, weightKg: 30.0, gender);

        Assert.True(r2.Bmi > r1.Bmi);

        // age>0 なので null ではないはず
        Assert.NotNull(r1.ObesityDegree);
        Assert.NotNull(r2.ObesityDegree);
        Assert.True(r2.ObesityDegree!.Value > r1.ObesityDegree!.Value);
    }
}
