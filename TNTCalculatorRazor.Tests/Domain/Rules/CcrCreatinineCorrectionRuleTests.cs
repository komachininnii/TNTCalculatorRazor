using System;
using System.Collections.Generic;
using System.Text;
using TNTCalculatorRazor.Domain.Enums;
using TNTCalculatorRazor.Domain.Rules;
using Xunit;

namespace TNTCalculatorRazor.Tests.Domain.Rules;

public class CcrCreatinineCorrectionRuleTests
{
    [Theory]
    // 年齢が70未満なら補正なし
    [InlineData(69, GenderType.Male, 0.1, CcrCreatinineCorrectionType.None, null, "")]
    [InlineData(69, GenderType.Female, 0.1, CcrCreatinineCorrectionType.None, null, "")]
    public void 年齢が70未満は補正なし( int age, GenderType gender, double cr,
        CcrCreatinineCorrectionType expectedType, double? expectedCorrected, string expectedNote )
    {
        var type = CcrCreatinineCorrectionRule.GetType(age, gender, cr);
        var corrected = CcrCreatinineCorrectionRule.GetCorrectedCreatinine(age, gender, cr);
        var note = CcrCreatinineCorrectionRule.GetNote(type);

        Assert.Equal(expectedType, type);
        Assert.Equal(expectedCorrected, corrected);
        Assert.Equal(expectedNote, note);
    }

    [Theory]
    // 男性：70以上かつCr<0.8で補正
    [InlineData(70, GenderType.Male, 0.79, CcrCreatinineCorrectionType.Male70Plus_Min08, 0.8, "※補正(Cr0.8)")]
    [InlineData(90, GenderType.Male, 0.10, CcrCreatinineCorrectionType.Male70Plus_Min08, 0.8, "※補正(Cr0.8)")]
    // 男性：ちょうど0.8は補正しない（<なので）
    [InlineData(70, GenderType.Male, 0.80, CcrCreatinineCorrectionType.None, null, "")]
    public void 男性_70以上_Cr閾値で補正判定( int age, GenderType gender, double cr,
        CcrCreatinineCorrectionType expectedType, double? expectedCorrected, string expectedNote )
    {
        var type = CcrCreatinineCorrectionRule.GetType(age, gender, cr);
        var corrected = CcrCreatinineCorrectionRule.GetCorrectedCreatinine(age, gender, cr);
        var note = CcrCreatinineCorrectionRule.GetNote(type);

        Assert.Equal(expectedType, type);
        Assert.Equal(expectedCorrected, corrected);
        Assert.Equal(expectedNote, note);
    }

    [Theory]
    // 女性：70以上かつCr<0.6で補正
    [InlineData(70, GenderType.Female, 0.59, CcrCreatinineCorrectionType.Female70Plus_Min06, 0.6, "※補正(Cr0.6)")]
    [InlineData(90, GenderType.Female, 0.10, CcrCreatinineCorrectionType.Female70Plus_Min06, 0.6, "※補正(Cr0.6)")]
    // 女性：ちょうど0.6は補正しない（<なので）
    [InlineData(70, GenderType.Female, 0.60, CcrCreatinineCorrectionType.None, null, "")]
    public void 女性_70以上_Cr閾値で補正判定( int age, GenderType gender, double cr,
        CcrCreatinineCorrectionType expectedType, double? expectedCorrected, string expectedNote )
    {
        var type = CcrCreatinineCorrectionRule.GetType(age, gender, cr);
        var corrected = CcrCreatinineCorrectionRule.GetCorrectedCreatinine(age, gender, cr);
        var note = CcrCreatinineCorrectionRule.GetNote(type);

        Assert.Equal(expectedType, type);
        Assert.Equal(expectedCorrected, corrected);
        Assert.Equal(expectedNote, note);
    }
}
