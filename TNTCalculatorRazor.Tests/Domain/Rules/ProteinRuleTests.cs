using System;
using System.Collections.Generic;
using System.Text;
using TNTCalculatorRazor.Domain.Enums;
using TNTCalculatorRazor.Domain.Rules;
using Xunit;

namespace TNTCalculatorRazor.Tests.Domain.Rules;

public class ProteinRuleTests
{
    // 成人で「例外疾患」として扱う DiseaseType の一覧（仕様固定）
    [Theory]
    [InlineData(DiseaseType.RenalFailure)]
    [InlineData(DiseaseType.Hemodialysis)]
    [InlineData(DiseaseType.LiverCirrhosis)]
    public void 成人_例外疾患は_標準体重使用_かつ_ストレス係数無視( DiseaseType disease )
    {
        Assert.True(ProteinRule.UseStandardWeightForProtein(age: 40, disease: disease));
        Assert.True(ProteinRule.IsStressFactorIgnored(age: 40, disease: disease));
    }

    [Fact]
    public void 成人_Noneは_標準体重使用しない_かつ_ストレス係数無視しない()
    {
        Assert.False(ProteinRule.UseStandardWeightForProtein(age: 40, disease: DiseaseType.None));
        Assert.False(ProteinRule.IsStressFactorIgnored(age: 40, disease: DiseaseType.None));
    }

    [Theory]
    [InlineData(DiseaseType.RenalFailure)]
    [InlineData(DiseaseType.Hemodialysis)]
    [InlineData(DiseaseType.LiverCirrhosis)]
    public void 小児は_例外疾患でも_標準体重使用しない_かつ_ストレス係数無視しない( DiseaseType disease )
    {
        Assert.False(ProteinRule.UseStandardWeightForProtein(age: 10, disease: disease));
        Assert.False(ProteinRule.IsStressFactorIgnored(age: 10, disease: disease));
    }
}
