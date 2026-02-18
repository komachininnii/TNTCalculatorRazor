using System;
using System.Collections.Generic;
using System.Text;
using TNTCalculatorRazor.Domain.Enums;
using TNTCalculatorRazor.Domain.Tables;
using TNTCalculatorRazor.Tests.TestData;
using Xunit;

namespace TNTCalculatorRazor.Tests.Domain.Tables;

public sealed class EnteralPackageTableTests
{

    [Theory]
    [MemberData(nameof(EnteralFormulaTestCases.CurrentFormulas), MemberType = typeof(EnteralFormulaTestCases))]
    public void Get_現行製剤で取得でき_規格は正で昇順かつ重複なし( EnteralFormulaType type )
    {
        var vols = EnteralPackageTable.Get(type);

        Assert.NotNull(vols);
        Assert.NotEmpty(vols);

        Assert.All(vols, v => Assert.True(v > 0));

        // Get内でOrderByしているので昇順のはず
        Assert.True(vols.SequenceEqual(vols.OrderBy(x => x)));

        Assert.Equal(vols.Count, vols.Distinct().Count());
    }

    [Fact]
    public void Inorasは旧WebForms互換で187固定()
    {
        var vols = EnteralPackageTable.Get(EnteralFormulaType.Inoras16);

        Assert.Single(vols);
        Assert.Equal(187, vols[0]);
    }

    [Fact]
    public void 未登録の製剤は例外()
    {
        // enumに存在しない値を無理やり作る（将来の追加漏れ検知にも効く）
        var unknown = (EnteralFormulaType)(-1);

        var ex = Assert.Throws<InvalidOperationException>(() => EnteralPackageTable.Get(unknown));
        Assert.Contains("未登録", ex.Message);
    }
}
