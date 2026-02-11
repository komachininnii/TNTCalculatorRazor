using System;
using System.Collections.Generic;
using System.Text;
using TNTCalculatorRazor.Domain.Calculators;
using TNTCalculatorRazor.Domain.Enums;
using Xunit;

namespace TNTCalculatorRazor.Tests.Domain.Calculators;

public class WaterCalculatorTests
{
    //=========================
    // CalculateBase
    //=========================

    [Fact]
    public void CalculateBase_透析が最優先で15mLkg_実測体重を使う()
    {
        // Arrange
        double actualWeight = 60.0;

        // Act（乳児・妊娠・肥満度などがどうでも、透析が最優先）
        var ml = WaterCalculator.CalculateBase(
            age: 0,
            actualWeight: actualWeight,
            isHemodialysis: true,
            isPregnant: true,
            adjustedWeight: 50.0,
            obesityDegree: 200.0);

        // Assert
        Assert.Equal(15.0 * actualWeight, ml, 6);
    }

    [Fact]
    public void CalculateBase_乳児は150mLkg_実測体重を使う()
    {
        // Arrange
        double w = 8.0;

        // Act
        var ml = WaterCalculator.CalculateBase(
            age: 0,
            actualWeight: w,
            isHemodialysis: false,
            isPregnant: false,
            adjustedWeight: 0,
            obesityDegree: 0);

        // Assert
        Assert.Equal(150.0 * w, ml, 6);
    }

    [Theory]
    [InlineData(17, 9.99, 999.0)]   // <10kg: 100*w
    [InlineData(17, 10.0, 1000.0)]   // 10kgちょうど: 1000 + (w-10)*50
    [InlineData(17, 19.99, 1499.5)]  // <20kg: 1000 + (w-10)*50
    [InlineData(17, 20.0, 1500.0)]   // 20kgちょうど: 1500 + (w-20)*20
    [InlineData(17, 25.0, 1600.0)]   // >=20kg: 1500 + (w-20)*20
    public void CalculateBase_小児HollidaySegarが正しく動作する( int age, double w, double expectedMl )
    {
        var ml = WaterCalculator.CalculateBase(
            age: age,
            actualWeight: w,
            isHemodialysis: false,
            isPregnant: false,
            adjustedWeight: 0,
            obesityDegree: 0);

        Assert.Equal(expectedMl, ml, 6);
    }

    [Theory]
    [InlineData(18, 60.0, 50.0, 119.9, 2100.0)] // 非該当：実測 35*60
    [InlineData(18, 60.0, 50.0, 120.0, 1750.0)] // 該当：調整 35*50
    public void CalculateBase_成人_妊娠かつ肥満度120以上で調整体重を使う(
        int age,
        double actualW,
        double adjustedW,
        double obesityDegree,
        double expectedMl )
    {
        var ml = WaterCalculator.CalculateBase(
            age: age,
            actualWeight: actualW,
            isHemodialysis: false,
            isPregnant: true,
            adjustedWeight: adjustedW,
            obesityDegree: obesityDegree);

        Assert.Equal(expectedMl, ml, 6);
    }

    [Theory]
    [InlineData(55, 60.0, 2100.0)] // <=55: 35*w
    [InlineData(56, 60.0, 1800.0)] // 56-65: 30*w
    [InlineData(65, 60.0, 1800.0)] // <=65: 30*w
    [InlineData(66, 60.0, 1500.0)] // >=66: 25*w
    public void CalculateBase_成人_年齢帯ごとの係数が正しく動作する( int age, double w, double expectedMl )
    {
        var ml = WaterCalculator.CalculateBase(
            age: age,
            actualWeight: w,
            isHemodialysis: false,
            isPregnant: false,
            adjustedWeight: 0,
            obesityDegree: 0);

        Assert.Equal(expectedMl, ml, 6);
    }

    //=========================
    // CalculateFeverCorrection
    //=========================

    [Theory]
    [InlineData(BodyTemperatureLevel.Normal, 10.0, 0.0)]
    [InlineData(BodyTemperatureLevel.Normal, 80.0, 0.0)]
    public void CalculateFeverCorrection_平熱は常に0( BodyTemperatureLevel level, double w, double expected )
    {
        var ml = WaterCalculator.CalculateFeverCorrection(level, w);
        Assert.Equal(expected, ml, 6);
    }

    [Theory]
    // <15kg: step * 10mL/kg
    [InlineData(BodyTemperatureLevel.Fever37, 14.9, 1 * 10.0 * 14.9)]
    [InlineData(BodyTemperatureLevel.Fever38, 10.0, 2 * 10.0 * 10.0)]
    [InlineData(BodyTemperatureLevel.Fever39, 1.0, 3 * 10.0 * 1.0)]
    [InlineData(BodyTemperatureLevel.Fever40, 14.0, 4 * 10.0 * 14.0)]
    public void CalculateFeverCorrection_15kg未満は10mLkg_step倍(
        BodyTemperatureLevel level, double w, double expected )
    {
        var ml = WaterCalculator.CalculateFeverCorrection(level, w);
        Assert.Equal(expected, ml, 6);
    }

    [Theory]
    // >=15kg: step * 150mL
    [InlineData(BodyTemperatureLevel.Fever37, 15.0, 1 * 150.0)]
    [InlineData(BodyTemperatureLevel.Fever38, 80.0, 2 * 150.0)]
    [InlineData(BodyTemperatureLevel.Fever39, 20.0, 3 * 150.0)]
    [InlineData(BodyTemperatureLevel.Fever40, 50.0, 4 * 150.0)]
    public void CalculateFeverCorrection_15kg以上は150mL_step倍(
        BodyTemperatureLevel level, double w, double expected )
    {
        var ml = WaterCalculator.CalculateFeverCorrection(level, w);
        Assert.Equal(expected, ml, 6);
    }

    //=========================
    // CalculateTotal
    //=========================

    [Theory]
    [InlineData(true, BodyTemperatureLevel.Normal)]
    [InlineData(true, BodyTemperatureLevel.Fever40)]
    [InlineData(false, BodyTemperatureLevel.Normal)]
    public void CalculateTotal_補正が入らない条件ではBaseのみになる( bool isHemodialysis, BodyTemperatureLevel temp )
    {
        double w = 60.0;

        var total = WaterCalculator.CalculateTotal(
            age: 70,
            actualWeight: w,
            isHemodialysis: isHemodialysis,
            isPregnant: false,
            adjustedWeight: 50.0,
            obesityDegree: 200.0,
            temperatureLevel: temp);

        var baseOnly = WaterCalculator.CalculateBase(
            age: 70,
            actualWeight: w,
            isHemodialysis: isHemodialysis,
            isPregnant: false,
            adjustedWeight: 50.0,
            obesityDegree: 200.0);

        Assert.Equal(baseOnly, total, 6);
    }

    [Theory]
    [InlineData(BodyTemperatureLevel.Fever37)]
    [InlineData(BodyTemperatureLevel.Fever38)]
    [InlineData(BodyTemperatureLevel.Fever39)]
    [InlineData(BodyTemperatureLevel.Fever40)]
    public void CalculateTotal_非透析かつ平熱以外では発熱補正が加算される( BodyTemperatureLevel temp )
    {
        double w = 20.0;

        var total = WaterCalculator.CalculateTotal(
            age: 30,
            actualWeight: w,
            isHemodialysis: false,
            isPregnant: false,
            adjustedWeight: 0,
            obesityDegree: 0,
            temperatureLevel: temp);

        // base (age<=55): 35*w = 700
        // fever (w>=15): step*150
        var expected = (35.0 * w) + ((int)temp * 150.0);

        Assert.Equal(expected, total, 6);
    }
}
