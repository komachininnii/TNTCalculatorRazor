using System.Reflection;
using Microsoft.Extensions.Options;
using TNTCalculatorRazor.Domain.Calculators;
using TNTCalculatorRazor.Domain.Enums;
using TNTCalculatorRazor.Domain.Models;
using TNTCalculatorRazor.Domain.Rules;
using TNTCalculatorRazor.Pages;
using Xunit;

namespace TNTCalculatorRazor.Tests.Integration;

public class IndexIntegrationTests
{
    // NOTE（余力枠）:
    // UI/Index の統合テストは原則最小限。
    // Domainの単体テストで計算ロジックを担保しつつ、
    // ここでは CorrectedBmrBased の最重要契約だけを smoke test として固定する。
    // HTTP依存を避けるため private の再計算メソッドを reflection で呼ぶ。

    [Fact]
    // 統合テスト
    public void CorrectedBmrBased_では_EnergyFinalとCorrectedBmrEnergyDisplayKcalが一致する()
    {
        // Arrange
        var options = Options.Create(new InternalManualOptions { Enabled = false, Url = "" });
        var page = new IndexModel(options);

        page.Age = 30;
        page.Height = 170.0;
        page.Weight = 60.0;
        page.Gender = GenderType.Male;

        page.SelectedEnergyOrder = EnergyOrderType.CorrectedBmrBased;

        // 安全側に倒して必須っぽいenumを埋める（存在するものだけ）
        page.SelectedDisease = DiseaseType.None;
        // page.SelectedProteinCorrection = ProteinCorrectionType.None; // 必要なら

        // Act: HTTP依存を避けて内部再計算を直接呼ぶ
        InvokePrivate(page, "RecalcAll");

        // Assert
        Assert.True(page.CorrectedBmrEnergyDisplayKcal.HasValue);
        Assert.True(page.EnergyFinal.HasValue);
        Assert.Equal(page.CorrectedBmrEnergyDisplayKcal!.Value, page.EnergyFinal!.Value);
    }

    [Fact]
    public void 成人腎疾患では_蛋白は標準体重を使い_Energyは補正体重連動のまま計算される()
    {
        var page = CreatePage();
        page.Age = 40;
        page.Height = 170.0;
        page.Weight = 90.0;
        page.Gender = GenderType.Male;
        // UI同期後状態を前提に、疾患と蛋白補正を明示セット
        page.SelectedDisease = DiseaseType.RenalFailure;
        page.SelectedProteinCorrection = ProteinCorrectionType.CKD3bTo5;
        page.SelectedEnergyOrder = EnergyOrderType.CorrectedBmrBased;

        InvokePrivate(page, "RecalcAll");

        Assert.Equal(BmrWeightBasisType.Adjusted, page.CorrectedBmrWeightBasis);
        Assert.NotNull(page.BodyIndex);
        Assert.NotNull(page.CorrectedWeight);
        Assert.NotNull(page.ProteinFinal);
        Assert.NotNull(page.EnergyFinal);
        Assert.Equal(ProteinCorrectionType.CKD3bTo5, page.SelectedProteinCorrection);

        var standardWeight = page.BodyIndex!.StandardWeight;
        var expectedProtein = RoundingRules.RoundGram1dp(
            ProteinCalculator.Calculate(
                age: page.Age!.Value,
                weightForProtein: standardWeight,
                stressFactor: page.StressTotal,
                proteinCorrect: 0.7,
                disease: page.SelectedDisease));

        Assert.Equal(expectedProtein, page.ProteinFinal);
        Assert.NotEqual(standardWeight, page.CorrectedWeight!.Value);
        Assert.Equal(page.CorrectedBmrEnergyDisplayKcal, page.EnergyFinal);
    }

    [Fact]
    public void 発熱と褥瘡補正があると_StressTotalと最終Energyが一貫して増加する()
    {
        var baseline = CreatePage();
        baseline.Age = 35;
        baseline.Height = 165.0;
        baseline.Weight = 60.0;
        baseline.Gender = GenderType.Female;
        baseline.SelectedEnergyOrder = EnergyOrderType.CorrectedBmrBased;
        baseline.ActivityFactor = ActivityFactorType.Sitting;
        baseline.StressFactor = StressFactorType.MildStress;

        InvokePrivate(baseline, "RecalcAll");

        var stressed = CreatePage();
        stressed.Age = baseline.Age;
        stressed.Height = baseline.Height;
        stressed.Weight = baseline.Weight;
        stressed.Gender = baseline.Gender;
        stressed.SelectedEnergyOrder = baseline.SelectedEnergyOrder;
        stressed.ActivityFactor = baseline.ActivityFactor;
        stressed.StressFactor = baseline.StressFactor;
        stressed.SelectedBodyTemperature = BodyTemperatureLevel.Fever39;
        stressed.SelectedPressureUlcer = PressureUlcerLevel.D3;

        InvokePrivate(stressed, "RecalcAll");

        Assert.Equal(baseline.StressTotal + 0.8, stressed.StressTotal, precision: 5);
        Assert.True(stressed.EnergyFinal > baseline.EnergyFinal);

        var expectedEnergy = RoundingRules.RoundKcalToInt(
            BmrCalculator.Calculate(
                stressed.Age!.Value,
                stressed.CorrectedWeight!.Value,
                stressed.Height!.Value,
                stressed.Gender).RawValue
            * 1.2 // ActivityFactorType.Sitting
            * stressed.StressTotal);

        Assert.Equal(expectedEnergy, stressed.EnergyFinal);
    }

    [Fact]
    public void 不正なProteinCorrection値でも_RecalcAllは完走し_Noneへ正規化される()
    {
        var page = CreatePage();
        page.Age = 45;
        page.Height = 170.0;
        page.Weight = 70.0;
        page.Gender = GenderType.Male;
        page.SelectedDisease = DiseaseType.None;
        page.SelectedEnergyOrder = EnergyOrderType.CorrectedBmrBased;
        page.SelectedProteinCorrection = (ProteinCorrectionType)999;

        var ex = Record.Exception(() => InvokePrivate(page, "RecalcAll"));

        Assert.Null(ex);
        Assert.Equal(ProteinCorrectionType.None, page.SelectedProteinCorrection);
        Assert.NotNull(page.ProteinFinal);
    }

    [Fact]
    public void 不正なGender値でも_RecalcAllは完走し_Maleへ正規化される()
    {
        var page = CreatePage();
        page.Age = 45;
        page.Height = 170.0;
        page.Weight = 70.0;
        page.Gender = (GenderType)999;
        page.SelectedDisease = DiseaseType.None;
        page.SelectedEnergyOrder = EnergyOrderType.CorrectedBmrBased;
        page.SelectedProteinCorrection = ProteinCorrectionType.None;

        var ex = Record.Exception(() => InvokePrivate(page, "RecalcAll"));

        Assert.Null(ex);
        Assert.Equal(GenderType.Male, page.Gender);
        Assert.NotNull(page.ProteinFinal);
    }

    private static IndexModel CreatePage()
    {
        var options = Options.Create(new InternalManualOptions { Enabled = false, Url = "" });
        return new IndexModel(options);
    }

    private static void InvokePrivate( object obj, string methodName )
    {
        var mi = obj.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.True(mi != null, $"privateメソッド '{methodName}' が見つかりません。");
        mi!.Invoke(obj, null);
    }
}
