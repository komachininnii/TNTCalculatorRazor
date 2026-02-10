using System;
using System.Collections.Generic;
using System.Text;
using TNTCalculatorRazor.Domain.Enums;
using TNTCalculatorRazor.Domain.Selectors;
using Xunit;

namespace TNTCalculatorRazor.Tests.Domain.Selectors;

public class WeightForCalculationSelectorTests
{
    // ==========================
    // Energy
    // ==========================

    [Fact]
    public void Select_Energy_Age0_ActualWeightを使用()
    {
        var w = WeightForCalculationSelector.Select(
            usage: WeightUsage.Energy,
            age: 0,
            actualWeight: 8.0,
            correctedWeight: 6.5,
            standardWeight: 7.0,
            disease: DiseaseType.None);

        Assert.Equal(8.0, w);
    }

    [Fact]
    public void Select_Energy_Age1以上_CorrectedWeightを使用()
    {
        var w = WeightForCalculationSelector.Select(
            usage: WeightUsage.Energy,
            age: 1,
            actualWeight: 10.0,
            correctedWeight: 9.2,
            standardWeight: 9.5,
            disease: DiseaseType.None);

        Assert.Equal(9.2, w);
    }

    // ==========================
    // Protein
    // ==========================

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(17)]
    public void Select_Protein_小児は常にActualWeightを使用( int age )
    {
        var w = WeightForCalculationSelector.Select(
            usage: WeightUsage.Protein,
            age: age,
            actualWeight: 20.0,
            correctedWeight: 18.0,
            standardWeight: 19.0,
            disease: DiseaseType.None);

        Assert.Equal(20.0, w);
    }

    [Theory]
    [InlineData(DiseaseType.RenalFailure)]
    [InlineData(DiseaseType.Hemodialysis)]
    [InlineData(DiseaseType.LiverCirrhosis)]
    public void Select_Protein_成人_例外疾患はStandardWeightを使用( DiseaseType disease )
    {
        var w = WeightForCalculationSelector.Select(
            usage: WeightUsage.Protein,
            age: 40,
            actualWeight: 70.0,
            correctedWeight: 65.0,
            standardWeight: 60.0,
            disease: disease);

        Assert.Equal(60.0, w);
    }

    [Fact]
    public void Select_Protein_成人_通常はCorrectedWeightを使用()
    {
        var w = WeightForCalculationSelector.Select(
            usage: WeightUsage.Protein,
            age: 40,
            actualWeight: 70.0,
            correctedWeight: 65.0,
            standardWeight: 60.0,
            disease: DiseaseType.None);

        Assert.Equal(65.0, w);
    }

    // ==========================
    // Defensive
    // ==========================

    [Fact]
    public void Select_未知のWeightUsageは例外()
    {
        var invalid = (WeightUsage)999;

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            WeightForCalculationSelector.Select(
                usage: invalid,
                age: 30,
                actualWeight: 70.0,
                correctedWeight: 65.0,
                standardWeight: 60.0,
                disease: DiseaseType.None));
    }
}
