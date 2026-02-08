using System;
using System.Collections.Generic;
using System.Text;
using TNTCalculatorRazor.Domain.Rules;
using Xunit;

namespace TNTCalculatorRazor.Tests.Domain.Rules;

/// <summary>
/// RoundingRules クラスのテスト
/// </summary>
public class RoundingRulesTests
{
    /// <summary>
    /// エネルギー（kcal）の四捨五入テスト
    /// </summary>
    [Theory]
    [InlineData(1234.0, 1234)]
    [InlineData(1234.4, 1234)]  // 0.4 → 切り捨て
    [InlineData(1234.5, 1235)]  // 0.5 → 切り上げ
    [InlineData(1234.6, 1235)]  // 0.6 → 切り上げ
    [InlineData(1235.5, 1236)]  // 奇数.5 → 切り上げ（AwayFromZero）
    public void RoundKcalToInt_四捨五入が正しく動作する( double input, int expected )
    {
        // Act
        var actual = RoundingRules.RoundKcalToInt(input);

        // Assert
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// 蛋白質（g）の小数第1位四捨五入テスト
    /// </summary>
    [Theory]
    [InlineData(12.34, 12.3)]   // 0.04 → 切り捨て
    [InlineData(12.35, 12.4)]   // 0.05 → 切り上げ
    [InlineData(12.36, 12.4)]   // 0.06 → 切り上げ
    [InlineData(45.67, 45.7)]
    [InlineData(45.64, 45.6)]
    public void RoundGram1dp_小数第1位の四捨五入が正しく動作する( double input, double expected )
    {
        // Act
        var actual = RoundingRules.RoundGram1dp(input);

        // Assert
        Assert.Equal(expected, actual, precision: 1);  // 浮動小数点誤差を考慮
    }

    /// <summary>
    /// 水分（mL）の切り上げテスト
    /// </summary>
    [Theory]
    [InlineData(100.0, 100)]    // ちょうど → そのまま
    [InlineData(100.1, 101)]    // 0.1 → 切り上げ
    [InlineData(100.5, 101)]    // 0.5 → 切り上げ
    [InlineData(100.9, 101)]    // 0.9 → 切り上げ
    [InlineData(1500.01, 1501)] // 大きな値でも
    public void CeilMl_切り上げが正しく動作する( double input, int expected )
    {
        // Act
        var actual = RoundingRules.CeilMl(input);

        // Assert
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// 経腸栄養投与量（mL）の四捨五入テスト
    /// </summary>
    [Theory]
    [InlineData(250.4, 250)]
    [InlineData(250.5, 251)]
    [InlineData(250.6, 251)]
    [InlineData(1000.5, 1001)]
    public void RoundEnteralMl_四捨五入が正しく動作する( double input, int expected )
    {
        // Act
        var actual = RoundingRules.RoundEnteralMl(input);

        // Assert
        Assert.Equal(expected, actual);
    }
}
