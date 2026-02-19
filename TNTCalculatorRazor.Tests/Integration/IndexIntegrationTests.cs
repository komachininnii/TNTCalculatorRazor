using System.Reflection;
using Microsoft.Extensions.Options;
using TNTCalculatorRazor.Domain.Enums;
using TNTCalculatorRazor.Domain.Models;
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

    private static void InvokePrivate( object obj, string methodName )
    {
        var mi = obj.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.True(mi != null, $"privateメソッド '{methodName}' が見つかりません。");
        mi!.Invoke(obj, null);
    }
}
