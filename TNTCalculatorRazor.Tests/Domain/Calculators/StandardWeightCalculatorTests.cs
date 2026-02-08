using System;
using System.Collections.Generic;
using System.Text;
using TNTCalculatorRazor.Domain.Calculators;
using TNTCalculatorRazor.Domain.Enums;
using Xunit;

namespace TNTCalculatorRazor.Tests.Domain.Calculators;

/// <summary>
/// StandardWeightCalculator の現行実装（2026-02 時点）を固定する回帰テスト。
/// 小児・乳児は実装結果を正とし、成人は BMI22 の仕様を検証する。
/// </summary>
public class StandardWeightCalculatorTests
{
    // ========================================
    // 乳児（Age < 6）
    // ========================================

    [Theory]
    [InlineData(0, 70.0, GenderType.Male, 8.5)]
    [InlineData(3, 90.0, GenderType.Male, 12.7)]
    [InlineData(5, 110.0, GenderType.Male, 18.6)]
    public void Calculate_乳児_男性_日本小児内分泌学会式( int age, double height, GenderType gender, double expected )
    {
        var result = StandardWeightCalculator.Calculate(age, height, gender);
        Assert.Equal(expected, result, precision: 1);
    }

    [Theory]
    [InlineData(0, 70.0, GenderType.Female, 8.2)]
    [InlineData(3, 90.0, GenderType.Female, 12.5)]
    [InlineData(5, 110.0, GenderType.Female, 18.7)]
    public void Calculate_乳児_女性_日本小児内分泌学会式( int age, double height, GenderType gender, double expected )
    {
        var result = StandardWeightCalculator.Calculate(age, height, gender);
        Assert.Equal(expected, result, precision: 1);
    }

    // ========================================
    // 小児（6 <= Age < 18）- 身長 < 140cm
    // ========================================

    [Theory]
    [InlineData(6, 110.0, GenderType.Male, 18.0)]
    [InlineData(8, 120.0, GenderType.Male, 22.0)]
    [InlineData(10, 130.0, GenderType.Male, 27.1)]
    [InlineData(11, 135.0, GenderType.Male, 30.0)]
    public void Calculate_小児_男性_身長140未満( int age, double height, GenderType gender, double expected )
    {
        var result = StandardWeightCalculator.Calculate(age, height, gender);
        Assert.Equal(expected, result, precision: 1);
    }

    [Theory]
    [InlineData(6, 110.0, GenderType.Female, 18.0)]
    [InlineData(8, 120.0, GenderType.Female, 21.9)]
    [InlineData(10, 130.0, GenderType.Female, 26.7)]
    [InlineData(11, 135.0, GenderType.Female, 29.7)]
    public void Calculate_小児_女性_身長140未満( int age, double height, GenderType gender, double expected )
    {
        var result = StandardWeightCalculator.Calculate(age, height, gender);
        Assert.Equal(expected, result, precision: 1);
    }

    // ========================================
    // 小児（6 <= Age < 18）- 140 <= 身長 < 149cm
    // ========================================

    [Theory]
    [InlineData(12, 140.0, GenderType.Male, 33.3)]
    [InlineData(13, 145.0, GenderType.Male, 37.0)]
    [InlineData(14, 148.0, GenderType.Male, 39.2)]
    public void Calculate_小児_男性_身長140以上149未満( int age, double height, GenderType gender, double expected )
    {
        var result = StandardWeightCalculator.Calculate(age, height, gender);
        Assert.Equal(expected, result, precision: 1);
    }

    [Theory]
    [InlineData(12, 140.0, GenderType.Female, 33.2)]
    [InlineData(13, 145.0, GenderType.Female, 37.6)]
    [InlineData(14, 148.0, GenderType.Female, 41.0)]
    public void Calculate_小児_女性_身長140以上149未満( int age, double height, GenderType gender, double expected )
    {
        var result = StandardWeightCalculator.Calculate(age, height, gender);
        Assert.Equal(expected, result, precision: 1);
    }

    // ========================================
    // 小児（6 <= Age < 18）- 身長 >= 149cm
    // ========================================

    [Theory]
    [InlineData(15, 150.0, GenderType.Male, 40.6)]
    [InlineData(16, 160.0, GenderType.Male, 49.3)]
    [InlineData(17, 170.0, GenderType.Male, 58.3)]
    public void Calculate_小児_男性_身長149以上( int age, double height, GenderType gender, double expected )
    {
        var result = StandardWeightCalculator.Calculate(age, height, gender);
        Assert.Equal(expected, result, precision: 1);
    }

    [Theory]
    [InlineData(15, 150.0, GenderType.Female, 43.4)]
    [InlineData(16, 155.0, GenderType.Female, 47.9)]
    [InlineData(17, 160.0, GenderType.Female, 51.5)]
    public void Calculate_小児_女性_身長149以上( int age, double height, GenderType gender, double expected )
    {
        var result = StandardWeightCalculator.Calculate(age, height, gender);
        Assert.Equal(expected, result, precision: 1);
    }

    // ========================================
    // 成人（Age >= 18）- BMI 22
    // ========================================

    [Theory]
    [InlineData(18, 160.0, GenderType.Male, 56.3)]
    [InlineData(25, 165.0, GenderType.Female, 59.9)]
    [InlineData(30, 170.0, GenderType.Male, 63.6)]
    [InlineData(40, 175.0, GenderType.Female, 67.4)]
    [InlineData(50, 180.0, GenderType.Male, 71.3)]
    public void Calculate_成人_BMI22で計算( int age, double height, GenderType gender, double expected )
    {
        var result = StandardWeightCalculator.Calculate(age, height, gender);
        Assert.Equal(expected, result, precision: 1);
    }

    [Fact]
    public void Calculate_成人_BMI22の計算式が正確()
    {
        // 170cm の場合: 170 * 170 * 22 * 0.0001 = 63.58
        var result = StandardWeightCalculator.Calculate(30, 170.0, GenderType.Male);
        Assert.Equal(63.6, result, precision: 1);
    }

    // ========================================
    // 境界値テスト
    // ========================================

    [Fact]
    public void Calculate_乳児から小児への境界_Age6()
    {
        // Age = 5 → 乳児式
        var infant = StandardWeightCalculator.Calculate(5, 110.0, GenderType.Male);

        // Age = 6 → 小児式
        var child = StandardWeightCalculator.Calculate(6, 110.0, GenderType.Male);

        // 異なる式なので値が変わる
        Assert.NotEqual(infant, child);
    }

    [Fact]
    public void Calculate_小児から成人への境界_Age18()
    {
        // Age = 17 → 小児式
        var child = StandardWeightCalculator.Calculate(17, 170.0, GenderType.Male);

        // Age = 18 → 成人式（BMI 22）
        var adult = StandardWeightCalculator.Calculate(18, 170.0, GenderType.Male);

        // 成人は BMI 22 なので 63.6kg
        Assert.Equal(63.6, adult, precision: 1);

        // 小児式と成人式で値が異なる
        Assert.NotEqual(child, adult);
    }

    [Fact]
    public void Calculate_小児_身長140の境界()
    {
        // height = 139.9 → 140未満の式
        var under140 = StandardWeightCalculator.Calculate(10, 139.9, GenderType.Male);

        // height = 140.0 → 140以上の式
        var at140 = StandardWeightCalculator.Calculate(10, 140.0, GenderType.Male);

        // 異なる式なので値が変わる
        Assert.NotEqual(under140, at140);
    }

    [Fact]
    public void Calculate_小児_身長149の境界()
    {
        // height = 148.9 → 149未満の式
        var under149 = StandardWeightCalculator.Calculate(13, 148.9, GenderType.Male);

        // height = 149.0 → 149以上の式
        var at149 = StandardWeightCalculator.Calculate(13, 149.0, GenderType.Male);

        // 異なる式なので値が変わる
        Assert.NotEqual(under149, at149);
    }

    // ========================================
    // 性別による違いの確認
    // ========================================

    [Theory]
    [InlineData(3, 90.0)]    // 乳児
    [InlineData(8, 120.0)]   // 小児（140未満）
    [InlineData(13, 145.0)]  // 小児（140-149）
    [InlineData(16, 160.0)]  // 小児（149以上）
    public void Calculate_性別で結果が異なる( int age, double height )
    {
        var male = StandardWeightCalculator.Calculate(age, height, GenderType.Male);
        var female = StandardWeightCalculator.Calculate(age, height, GenderType.Female);

        // 性別で計算式が異なるので、結果も異なる
        Assert.NotEqual(male, female);
    }

    [Fact]
    public void Calculate_成人は性別に関係なく同じ()
    {
        // 成人は BMI 22 なので性別関係なし
        var male = StandardWeightCalculator.Calculate(30, 170.0, GenderType.Male);
        var female = StandardWeightCalculator.Calculate(30, 170.0, GenderType.Female);

        Assert.Equal(male, female);
    }

    // ========================================
    // 実際のユースケース（統合的なテスト）
    // ========================================

    [Fact]
    public void Calculate_典型的な小学生男子()
    {
        // 10歳、身長135cm
        var result = StandardWeightCalculator.Calculate(10, 135.0, GenderType.Male);

        // 30kg前後が期待される
        Assert.InRange(result, 28.0, 32.0);
    }

    [Fact]
    public void Calculate_典型的な中学生女子()
    {
        // 13歳、身長155cm
        var result = StandardWeightCalculator.Calculate(13, 155.0, GenderType.Female);

        // 45kg前後が期待される
        Assert.InRange(result, 43.0, 48.0);
    }

    [Fact]
    public void Calculate_成人女性標準体格()
    {
        // 30歳、女性、158cm
        // 158^2 * 22 * 0.0001 = 54.9208
        var result = StandardWeightCalculator.Calculate(30, 158.0, GenderType.Female);

        Assert.Equal(54.9, result, precision: 1);
    }
}
