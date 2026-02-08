using System;
using System.Collections.Generic;
using System.Text;
using TNTCalculatorRazor.Domain.Calculators;
using TNTCalculatorRazor.Domain.Enums;
using Xunit;

namespace TNTCalculatorRazor.Tests.Domain.Calculators;

/// <summary>
/// BmrCalculator クラスのテスト
/// </summary>
public class BmrCalculatorTests
{
    // ========================================
    // 乳児（Age = 0）
    // ========================================

    [Theory]
    [InlineData(5.0, GenderType.Male, 262.2)]      // (5.0 - 0.4) × 57 = 262.2
    [InlineData(8.0, GenderType.Female, 433.2)]    // (8.0 - 0.4) × 57 = 433.2
    [InlineData(10.0, GenderType.Male, 547.2)]     // (10.0 - 0.4) × 57 = 547.2
    public void Calculate_乳児_体重10kg以下_京都PICU式( double weight, GenderType gender, double expected )
    {
        // Act
        var result = BmrCalculator.Calculate(0, weight, 0, gender);

        // Assert
        Assert.Equal(expected, result.RawValue, precision: 1);
        Assert.Equal(BmrFormulaType.Infant_KyotoPICU, result.Formula);
    }

    [Theory]
    [InlineData(12.0, GenderType.Male, 628.3)]     // (12.0 + 8.6) × 30.5 = 628.3
    [InlineData(12.0, GenderType.Female, 618.0)]   // (12.0 + 8.6) × 30.0 = 618.0
    [InlineData(15.0, GenderType.Male, 719.8)]     // (15.0 + 8.6) × 30.5 = 719.8
    public void Calculate_乳児_体重10kg超_京都PICU式( double weight, GenderType gender, double expected )
    {
        // Act
        var result = BmrCalculator.Calculate(0, weight, 0, gender);

        // Assert
        Assert.Equal(expected, result.RawValue, precision: 1);
        Assert.Equal(BmrFormulaType.Infant_KyotoPICU, result.Formula);
    }

    // ========================================
    // 小児（Age 1-17）
    // ========================================

    [Theory]
    [InlineData(1, 10.0, GenderType.Male, 610.0)]    // 61.0 × 10 = 610
    [InlineData(2, 12.0, GenderType.Female, 716.4)]  // 59.7 × 12 = 716.4
    [InlineData(3, 15.0, GenderType.Male, 822.0)]    // 54.8 × 15 = 822
    [InlineData(10, 30.0, GenderType.Female, 1044.0)] // 34.8 × 30 = 1044
    [InlineData(15, 50.0, GenderType.Male, 1350.0)]  // 27.0 × 50 = 1350
    public void Calculate_小児_日本人食事摂取基準2010( int age, double weight, GenderType gender, double expected )
    {
        // Act
        var result = BmrCalculator.Calculate(age, weight, 0, gender);

        // Assert
        Assert.Equal(expected, result.RawValue, precision: 1);
        Assert.Equal(BmrFormulaType.Child_JapanDRI2010, result.Formula);
    }

    // ========================================
    // 成人（Age >= 18）- Harris-Benedict式
    // ========================================

    [Fact]
    public void Calculate_成人男性_標準体格_HarrisBenedict式()
    {
        // 30歳、男性、170cm、65kg
        // 期待値: 66.47 + (13.75 × 65) + (5.0 × 170) - (6.76 × 30)
        //       = 66.47 + 893.75 + 850 - 202.8
        //       = 1607.42

        var result = BmrCalculator.Calculate(30, 65.0, 170.0, GenderType.Male);

        Assert.Equal(1607.42, result.RawValue, precision: 2);
        Assert.Equal(BmrFormulaType.Adult_HarrisBenedict, result.Formula);
    }

    [Fact]
    public void Calculate_成人女性_標準体格_HarrisBenedict式()
    {
        // 25歳、女性、160cm、50kg
        // 期待値: 655.1 + (9.56 × 50) + (1.85 × 160) - (4.68 × 25)
        //       = 655.1 + 478 + 296 - 117
        //       = 1312.1

        var result = BmrCalculator.Calculate(25, 50.0, 160.0, GenderType.Female);

        Assert.Equal(1312.1, result.RawValue, precision: 2);
        Assert.Equal(BmrFormulaType.Adult_HarrisBenedict, result.Formula);
    }

    [Theory]
    [InlineData(40, 70.0, 175.0, GenderType.Male, 1633.57)]   // 修正：66.47 + 962.5 + 875 - 270.4
    [InlineData(50, 55.0, 158.0, GenderType.Female, 1239.2)]  // 修正：655.1 + 525.8 + 292.3 - 234
    public void Calculate_成人_HarrisBenedict式_複数パターン(
        int age, double weight, double height, GenderType gender, double expected )
    {
        var result = BmrCalculator.Calculate(age, weight, height, gender);

        Assert.Equal(expected, result.RawValue, precision: 2);
        Assert.Equal(BmrFormulaType.Adult_HarrisBenedict, result.Formula);
    }

    // ========================================
    // 成人（Age >= 18）- Ganpule式（低体重・低身長）
    // ========================================

    [Fact]
    public void Calculate_成人_低体重_Ganpule式()
    {
        // 条件: weight < 25 OR height < 151
        // 30歳、男性、160cm、20kg（低体重）

        var result = BmrCalculator.Calculate(30, 20.0, 160.0, GenderType.Male);

        Assert.Equal(BmrFormulaType.Adult_Ganpule2007, result.Formula);
        Assert.True(result.RawValue > 0);  // 正の値であることを確認
    }

    [Fact]
    public void Calculate_成人_低身長_Ganpule式()
    {
        // 30歳、女性、145cm、50kg（低身長）

        var result = BmrCalculator.Calculate(30, 50.0, 145.0, GenderType.Female);

        Assert.Equal(BmrFormulaType.Adult_Ganpule2007, result.Formula);
        Assert.True(result.RawValue > 0);
    }

    [Theory]
    [InlineData(25, 20.0, 160.0, GenderType.Male)]    // 低体重（< 25kg）
    [InlineData(25, 50.0, 145.0, GenderType.Female)]  // 低身長（< 151cm）
    [InlineData(25, 20.0, 145.0, GenderType.Male)]    // 両方
    public void Calculate_成人_Ganpule式適用条件( int age, double weight, double height, GenderType gender )
    {
        var result = BmrCalculator.Calculate(age, weight, height, gender);

        Assert.Equal(BmrFormulaType.Adult_Ganpule2007, result.Formula);
    }

    // ========================================
    // DisplayValue のテスト
    // ========================================

    [Fact]
    public void DisplayValue_四捨五入が正しく動作する()
    {
        // 30歳、男性、170cm、65kg
        var result = BmrCalculator.Calculate(30, 65.0, 170.0, GenderType.Male);

        // RawValue: 1607.42 → DisplayValue: 1607
        Assert.Equal(1607, result.DisplayValue);
    }

    [Theory]
    [InlineData(30, 65.5, 170.0, GenderType.Male, 1614)]  // 1614.17 → 1614
    [InlineData(30, 66.0, 170.0, GenderType.Male, 1621)]  // 1621.17 → 1621
    public void DisplayValue_複数パターンで四捨五入(
        int age, double weight, double height, GenderType gender, int expectedDisplay )
    {
        var result = BmrCalculator.Calculate(age, weight, height, gender);

        Assert.Equal(expectedDisplay, result.DisplayValue);
    }

    // ========================================
    // 境界値テスト
    // ========================================

    [Fact]
    public void Calculate_乳児から小児への境界_Age1()
    {
        // Age = 0 → 乳児式
        var infant = BmrCalculator.Calculate(0, 10.0, 0, GenderType.Male);
        Assert.Equal(BmrFormulaType.Infant_KyotoPICU, infant.Formula);

        // Age = 1 → 小児式
        var child = BmrCalculator.Calculate(1, 10.0, 0, GenderType.Male);
        Assert.Equal(BmrFormulaType.Child_JapanDRI2010, child.Formula);
    }

    [Fact]
    public void Calculate_小児から成人への境界_Age18()
    {
        // Age = 17 → 小児式
        var child = BmrCalculator.Calculate(17, 50.0, 0, GenderType.Male);
        Assert.Equal(BmrFormulaType.Child_JapanDRI2010, child.Formula);

        // Age = 18 → 成人式
        var adult = BmrCalculator.Calculate(18, 50.0, 170.0, GenderType.Male);
        Assert.True(
            adult.Formula == BmrFormulaType.Adult_HarrisBenedict ||
            adult.Formula == BmrFormulaType.Adult_Ganpule2007);
    }

    [Fact]
    public void Calculate_HarrisBenedictとGanpuleの境界_Weight25()
    {
        // weight = 24.9, height = 160 → Ganpule（weight < 25）
        var ganpule = BmrCalculator.Calculate(30, 24.9, 160.0, GenderType.Male);
        Assert.Equal(BmrFormulaType.Adult_Ganpule2007, ganpule.Formula);

        // weight = 25.0, height = 160 → Harris-Benedict（weight >= 25 AND height >= 151）
        var harris = BmrCalculator.Calculate(30, 25.0, 160.0, GenderType.Male);
        Assert.Equal(BmrFormulaType.Adult_HarrisBenedict, harris.Formula);
    }

    [Fact]
    public void Calculate_HarrisBenedictとGanpuleの境界_Height151()
    {
        // height = 150.9, weight = 50 → Ganpule（height < 151）
        var ganpule = BmrCalculator.Calculate(30, 50.0, 150.9, GenderType.Female);
        Assert.Equal(BmrFormulaType.Adult_Ganpule2007, ganpule.Formula);

        // height = 151.0, weight = 50 → Harris-Benedict（weight >= 25 AND height >= 151）
        var harris = BmrCalculator.Calculate(30, 50.0, 151.0, GenderType.Female);
        Assert.Equal(BmrFormulaType.Adult_HarrisBenedict, harris.Formula);
    }
}
