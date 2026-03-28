using System;
using System.Collections.Generic;
using System.Linq;
using TNTCalculatorRazor.Domain.Enums;
using TNTCalculatorRazor.Domain.Models;
using TNTCalculatorRazor.Domain.Tables;
using Xunit;

namespace TNTCalculatorRazor.Tests.Domain.Tables;

/// <summary>
/// 組成値は改訂で変わり得るため固定しない。
/// TestData に重複定義は持たず、enum 一覧に対して
/// EnteralFormulaTable の整合性・不変条件を確認する。
/// </summary>
public sealed class EnteralFormulaTableTests
{
    public static IEnumerable<object[]> AllFormulaTypes()
        => Enum.GetValues<EnteralFormulaType>()
               .Cast<EnteralFormulaType>()
               .Select(x => new object[] { x });

    [Theory]
    [MemberData(nameof(AllFormulaTypes))]
    public void 全製剤で_組成と規格を取得できる( EnteralFormulaType type )
    {
        var info = EnteralFormulaTable.Get(type);

        Assert.NotNull(info.Composition);
        Assert.NotNull(info.Packages);
        Assert.NotEmpty(info.Packages);
    }

    [Theory]
    [MemberData(nameof(AllFormulaTypes))]
    public void 係数は有限で_負にならず_VolumePerKcalは正( EnteralFormulaType type )
    {
        var c = EnteralFormulaTable.GetComposition(type);

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

        Assert.InRange(c.VolumePerKcal, double.Epsilon, 1.0);
    }

    [Theory]
    [MemberData(nameof(AllFormulaTypes))]
    public void 水分比は0から1の範囲( EnteralFormulaType type )
    {
        var c = EnteralFormulaTable.GetComposition(type);

        Assert.True(c.VolumePerKcal > 0.0);

        var ratio = c.WaterPerKcal / c.VolumePerKcal;
        Assert.InRange(ratio, 0.0, 1.0);
    }

    [Theory]
    [MemberData(nameof(AllFormulaTypes))]
    public void 全製剤で_規格を取得でき_規格は正で昇順かつ重複なし( EnteralFormulaType type )
    {
        var vols = EnteralFormulaTable.GetPackages(type);

        Assert.NotNull(vols);
        Assert.NotEmpty(vols);
        Assert.All(vols, v => Assert.True(v > 0));

        Assert.True(vols.SequenceEqual(vols.OrderBy(x => x)));
        Assert.Equal(vols.Count, vols.Distinct().Count());
    }

    [Fact]
    public void Inorasは旧WebForms互換で187固定()
    {
        var vols = EnteralFormulaTable.GetPackages(EnteralFormulaType.Inoras16);

        Assert.Single(vols);
        Assert.Equal(187, vols[0]);
    }

    [Fact]
    public void EnteralFormulaInfoはpackagesを防御的コピーして保持する()
    {
        var packages = new[] { 300, 400 };
        var info = new EnteralFormulaInfo(
            new EnteralFormulaComposition(1, 1, 1, 1, 1, 1, 1),
            packages);

        packages[0] = 999;

        Assert.Equal(new[] { 300, 400 }, info.Packages);
    }

    [Fact]
    public void 未登録の製剤は例外_将来の登録漏れ検知()
    {
        var unknown = (EnteralFormulaType)(-1);

        Assert.Throws<InvalidOperationException>(() => EnteralFormulaTable.GetComposition(unknown));
        Assert.Throws<InvalidOperationException>(() => EnteralFormulaTable.GetPackages(unknown));
    }
}
