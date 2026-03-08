using TNTCalculatorRazor.Domain.Enums;
using TNTCalculatorRazor.Domain.Selectors;
using Xunit;

namespace TNTCalculatorRazor.Tests.Domain.Selectors;

public class ProteinWeightSelectorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(17)]
    public void Select_小児は常にActualWeightを使用( int age )
    {
        var w = ProteinWeightSelector.Select(
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
    public void Select_成人_例外疾患はStandardWeightを使用( DiseaseType disease )
    {
        var w = ProteinWeightSelector.Select(
            age: 40,
            actualWeight: 70.0,
            correctedWeight: 65.0,
            standardWeight: 60.0,
            disease: disease);

        Assert.Equal(60.0, w);
    }

    [Fact]
    public void Select_成人_通常はCorrectedWeightを使用()
    {
        var w = ProteinWeightSelector.Select(
            age: 40,
            actualWeight: 70.0,
            correctedWeight: 65.0,
            standardWeight: 60.0,
            disease: DiseaseType.None);

        Assert.Equal(65.0, w);
    }

    [Fact]
    public void Select_Age18は成人扱い_通常はCorrectedWeight()
    {
        var w = ProteinWeightSelector.Select(
            age: 18,
            actualWeight: 70.0,
            correctedWeight: 65.0,
            standardWeight: 60.0,
            disease: DiseaseType.None);

        Assert.Equal(65.0, w);
    }
}
