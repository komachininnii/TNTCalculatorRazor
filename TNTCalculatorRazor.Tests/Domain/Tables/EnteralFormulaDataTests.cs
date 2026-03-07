using System;
using System.Linq;
using TNTCalculatorRazor.Domain.Enums;
using TNTCalculatorRazor.Domain.Tables;
using TNTCalculatorRazor.Tests.TestData;
using Xunit;

namespace TNTCalculatorRazor.Tests.Domain.Tables;

/// <summary>
/// 組成値は改訂で変わり得るため固定せず、入力ミス（kcal/容量の取り違え等）を主に検出する。
/// </summary>
public sealed class EnteralFormulaDataTests
{
    [Theory]
    [MemberData(nameof(EnteralFormulaTestCases.CurrentFormulas), MemberType = typeof(EnteralFormulaTestCases))]
    public void 現行製剤は_統合データから組成と規格を取得できる( EnteralFormulaType type )
    {
        var info = EnteralFormulaData.Get(type);

        Assert.NotNull(info.Composition);
        Assert.NotNull(info.Packages);
        Assert.NotEmpty(info.Packages);
    }

    [Theory]
    [MemberData(nameof(EnteralFormulaTestCases.CurrentFormulas), MemberType = typeof(EnteralFormulaTestCases))]
    public void 係数は有限で_負にならず_VolumePerKcalは正( EnteralFormulaType type )
    {
        var c = EnteralFormulaData.GetComposition(type);

        var values = new[]
        {
            c.VolumePerKcal,
            c.ProteinPerKcal,
            c.FatPerKcal,
            c.CarbPerKcal,
            c.SaltPerKcal,
            c.VitaminKPerKcal,
            c.WaterPerKcal
        };

        foreach (var v in values)
        {
            Assert.False(double.IsNaN(v));
            Assert.False(double.IsInfinity(v));
            Assert.True(v >= 0.0, $"Negative value detected: {v} ({type})");
        }

        // “kcal あたり”の体積が0はまずあり得ない（分母ミス/入力漏れ検知）
        Assert.InRange(c.VolumePerKcal, double.Epsilon, 1.0);
    }

    [Theory]
    [MemberData(nameof(EnteralFormulaTestCases.CurrentFormulas), MemberType = typeof(EnteralFormulaTestCases))]
    public void 水分比は0から1の範囲( EnteralFormulaType type )
    {
        var c = EnteralFormulaData.GetComposition(type);

        // Water/Volume は “容量中の水分割合” のはずなので 0～1 を期待
        Assert.True(c.VolumePerKcal > 0.0);

        var ratio = c.WaterPerKcal / c.VolumePerKcal;
        Assert.InRange(ratio, 0.0, 1.0);
    }

    // 明らかな入力ミス（分母・分子の取り違え）を拾う
    // packKcal と volumeMl の比が VolumePerKcal と一致することを確認する。
    [Theory]
    [MemberData(nameof(EnteralFormulaTestCases.CurrentFormulas_WithPackKcalAndVolume), MemberType = typeof(EnteralFormulaTestCases))]
    public void VolumePerKcalは_packKcalと容量mLの比に一致する_入力ミス検知(
        EnteralFormulaType type, double packKcal, double volumeMl )
    {
        var c = EnteralFormulaData.GetComposition(type);

        var expected = volumeMl / packKcal;

        // packKcal/volume の取り違えや、誤って別規格の分母を入れたミスを確実に検知したいので厳密一致。
        // いずれもテーブル側が "整数/整数" から計算されている前提（PerPack）なので、precisionで十分安定する。
        Assert.Equal(expected, c.VolumePerKcal, precision: 12);
    }

    [Theory]
    [MemberData(nameof(EnteralFormulaTestCases.CurrentFormulas), MemberType = typeof(EnteralFormulaTestCases))]
    public void 現行製剤で規格を取得でき_規格は正で昇順かつ重複なし( EnteralFormulaType type )
    {
        var vols = EnteralFormulaData.GetPackages(type);

        Assert.NotNull(vols);
        Assert.NotEmpty(vols);
        Assert.All(vols, v => Assert.True(v > 0));

        // GetPackages内でOrderByしているので昇順のはず
        Assert.True(vols.SequenceEqual(vols.OrderBy(x => x)));
        Assert.Equal(vols.Count, vols.Distinct().Count());
    }

    [Fact]
    public void Inorasは旧WebForms互換で187固定()
    {
        var vols = EnteralFormulaData.GetPackages(EnteralFormulaType.Inoras16);

        Assert.Single(vols);
        Assert.Equal(187, vols[0]);
    }

    [Fact]
    public void 未登録の製剤は例外_将来の登録漏れ検知()
    {
        var unknown = (EnteralFormulaType)(-1);

        Assert.Throws<InvalidOperationException>(() => EnteralFormulaData.GetComposition(unknown));
        Assert.Throws<InvalidOperationException>(() => EnteralFormulaData.GetPackages(unknown));
    }
}
