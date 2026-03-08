using System;
using TNTCalculatorRazor.Domain.Enums;
using TNTCalculatorRazor.Domain.Tables;
using Xunit;

namespace TNTCalculatorRazor.Tests.Domain.Tables;

public sealed class FactorAndStressTablesTests
{
    [Theory]
    [InlineData(ActivityFactorType.BedriddenComa, 1.0)]
    [InlineData(ActivityFactorType.Wheelchair, 1.3)]
    [InlineData(ActivityFactorType.Rehabilitation17, 1.7)]
    public void ActivityFactorTable_Get_正常な列挙値は期待値を返す( ActivityFactorType type, double expected )
    {
        var actual = ActivityFactorTable.Get(type);

        Assert.Equal(expected, actual, 6);
    }

    [Fact]
    public void ActivityFactorTable_Get_未定義値は例外をスローする()
    {
        var unknown = (ActivityFactorType)(-1);

        Assert.Throws<ArgumentOutOfRangeException>(() => ActivityFactorTable.Get(unknown));
    }

    [Theory]
    [InlineData(StressFactorType.Normal, 1.0)]
    [InlineData(StressFactorType.SurgeryMajor, 1.6)]
    [InlineData(StressFactorType.Burn100, 2.05)]
    public void StressFactorTable_Get_正常な列挙値は期待値を返す( StressFactorType type, double expected )
    {
        var actual = StressFactorTable.Get(type);

        Assert.Equal(expected, actual, 6);
    }

    [Fact]
    public void StressFactorTable_Get_未定義値は例外をスローする()
    {
        var unknown = (StressFactorType)(-1);

        Assert.Throws<ArgumentOutOfRangeException>(() => StressFactorTable.Get(unknown));
    }

    [Theory]
    [InlineData(BodyTemperatureLevel.Normal, 0.0)]
    [InlineData(BodyTemperatureLevel.Fever38, 0.4)]
    [InlineData(BodyTemperatureLevel.Fever40, 0.8)]
    public void TemperatureStressTable_GetAddition_正常な列挙値は期待値を返す( BodyTemperatureLevel level, double expected )
    {
        var actual = TemperatureStressTable.GetAddition(level);

        Assert.Equal(expected, actual, 6);
    }

    [Fact]
    public void TemperatureStressTable_GetAddition_未定義値は0を返す()
    {
        var unknown = (BodyTemperatureLevel)(-1);

        var actual = TemperatureStressTable.GetAddition(unknown);

        Assert.Equal(0.0, actual, 6);
    }

    [Theory]
    [InlineData(PressureUlcerLevel.None, 0.0)]
    [InlineData(PressureUlcerLevel.D3, 0.2)]
    [InlineData(PressureUlcerLevel.D5, 0.4)]
    public void PressureUlcerStressTable_GetAddition_正常な列挙値は期待値を返す( PressureUlcerLevel level, double expected )
    {
        var actual = PressureUlcerStressTable.GetAddition(level);

        Assert.Equal(expected, actual, 6);
    }

    [Fact]
    public void PressureUlcerStressTable_GetAddition_未定義値は0を返す()
    {
        var unknown = (PressureUlcerLevel)(-1);

        var actual = PressureUlcerStressTable.GetAddition(unknown);

        Assert.Equal(0.0, actual, 6);
    }
}
