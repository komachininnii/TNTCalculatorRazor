using System;
using System.Collections.Generic;
using System.Text;
using TNTCalculatorRazor.Domain.Enums;
using TNTCalculatorRazor.Domain.Tables;
using Xunit;

namespace TNTCalculatorRazor.Tests.Domain.Tables;

public sealed class EnergyOrderDefaultsTableTests
{
    [Theory]
    [InlineData(DiseaseType.None, EnergyOrderType.CorrectedBmrBased)]
    [InlineData(DiseaseType.Diabetes, EnergyOrderType.Kcal25)]
    [InlineData(DiseaseType.RenalFailure, EnergyOrderType.Kcal30)]
    [InlineData(DiseaseType.Hemodialysis, EnergyOrderType.Kcal30)]
    [InlineData(DiseaseType.LiverCirrhosis, EnergyOrderType.Kcal35)]
    public void GetDefault_代表疾患のデフォルトが仕様通り( DiseaseType disease, EnergyOrderType expected )
    {
        var actual = EnergyOrderDefaultsTable.GetDefault(disease);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetDefault_未知の疾患はCorrectedBmrBasedにフォールバック()
    {
        // 将来 enum が増えた時や、未想定値が来た時の挙動固定
        var unknown = (DiseaseType)(-1);

        var actual = EnergyOrderDefaultsTable.GetDefault(unknown);

        Assert.Equal(EnergyOrderType.CorrectedBmrBased, actual);
    }
}
