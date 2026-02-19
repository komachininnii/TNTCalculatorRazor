using System;
using System.Collections.Generic;
using System.Text;
using TNTCalculatorRazor.Domain.Services;
using Xunit;

namespace TNTCalculatorRazor.Tests.Domain.Services;

public sealed class EnteralPackageAllocatorTests
{
    // ----------------------------
    // 入力が不正 / 極端なケース
    // ----------------------------

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void BuildPlans_targetが0以下なら空( int target )
    {
        var plans = EnteralPackageAllocator.BuildPlans(
            targetVolumeMl: target,
            packageVolumes: new[] { 250, 200 });

        Assert.Empty(plans);
    }

    [Fact]
    public void BuildPlans_規格が全て不正なら空()
    {
        var plans = EnteralPackageAllocator.BuildPlans(
            targetVolumeMl: 1000,
            packageVolumes: new[] { 0, -200, 0 });

        Assert.Empty(plans);
    }

    // ----------------------------
    // 重要な不変条件（invariants）
    // ----------------------------

    [Theory]
    [InlineData(1000, new[] { 300, 400 })]   // Meibalance10
    [InlineData(850, new[] { 200, 267 })]    // PeptamenPrebio15
    [InlineData(187, new[] { 187 })]         // Inoras16
    [InlineData(999, new[] { 200, 400 })]    // GlucernaRex10
    public void BuildPlans_合計はtargetを超えない_かつ_remainder整合( int target, int[] volumes )
    {
        var plans = EnteralPackageAllocator.BuildPlans(target, volumes, maxPlans: 50);

        foreach (var p in plans)
        {
            Assert.InRange(p.TotalVolumeMl, 0, target);
            Assert.Equal(target - p.TotalVolumeMl, p.RemainderMl);
        }
    }

    [Theory]
    [InlineData(1000, new[] { 300, 400 })]
    [InlineData(850, new[] { 200, 267 })]
    [InlineData(999, new[] { 200, 400 })]
    public void BuildPlans_remainderは最小規格未満( int target, int[] volumes )
    {
        var minVol = volumes.Where(v => v > 0).Distinct().Min();

        var plans = EnteralPackageAllocator.BuildPlans(target, volumes, maxPlans: 50);

        foreach (var p in plans)
        {
            Assert.True(
                p.RemainderMl < minVol,
                $"Expected remainder < minVol. remainder={p.RemainderMl}, minVol={minVol}, target={target}");
        }
    }

    [Theory]
    [InlineData(1000, new[] { 300, 400 })]
    [InlineData(850, new[] { 200, 267 })]
    [InlineData(999, new[] { 200, 400, 200, 0, -1 })] // 重複・不正込み
    public void BuildPlans_counts合計がTotalVolumeと一致し_規格は入力由来のみ( int target, int[] volumes )
    {
        var set = volumes.Where(v => v > 0).Distinct().ToHashSet();

        var plans = EnteralPackageAllocator.BuildPlans(target, volumes, maxPlans: 50);

        foreach (var p in plans)
        {
            var sum = p.CountsByVolume.Sum(kv => kv.Key * kv.Value);
            Assert.Equal(sum, p.TotalVolumeMl);

            foreach (var kv in p.CountsByVolume)
            {
                Assert.Contains(kv.Key, set);
                Assert.True(kv.Value > 0);
            }
        }
    }

    // ----------------------------
    // 出力の安定性（順序・maxPlans・重複除去）
    // ----------------------------

    [Fact]
    public void BuildPlans_並び順はremainder優先_次に本数_そしてmaxPlansで打ち切り()
    {
        var target = 1000;
        var volumes = new[] { 300, 400 };

        var plans = EnteralPackageAllocator.BuildPlans(target, volumes, maxPlans: 3);

        Assert.True(plans.Count <= 3);

        for (int i = 0; i < plans.Count - 1; i++)
        {
            var a = plans[i];
            var b = plans[i + 1];

            if (a.RemainderMl != b.RemainderMl)
                Assert.True(a.RemainderMl <= b.RemainderMl);
            else
                Assert.True(a.TotalPackageCount <= b.TotalPackageCount);
        }
    }

    [Fact]
    public void BuildPlans_同じ内訳は重複して返さない()
    {
        var target = 1000;
        var volumes = new[] { 200, 400, 200, 0, -1 };

        var plans = EnteralPackageAllocator.BuildPlans(target, volumes, maxPlans: 50);

        string Key( EnteralPackagePlan p ) =>
            string.Join("|", p.CountsByVolume.OrderByDescending(k => k.Key)
                .Select(kv => $"{kv.Key}x{kv.Value}")) + $":r{p.RemainderMl}";

        var keys = plans.Select(Key).ToList();

        Assert.Equal(keys.Count, keys.Distinct().Count());
    }

    // ----------------------------
    // “このデータだとこうなる” のスナップショット1本
    // （性質テストより壊れやすいので最小に）
    // ----------------------------

    [Fact]
    public void BuildPlans_2規格の代表例で最良候補が期待通り()
    {
        // 例：1000mLを 400/300 で割付（余り最小→本数最小）
        var plans = EnteralPackageAllocator.BuildPlans(1000, new[] { 300, 400 }, maxPlans: 5);

        Assert.NotEmpty(plans);

        var best = plans[0];
        Assert.True(best.RemainderMl >= 0);

        // 1000を超えない範囲で余り最小（この規格では余り0が可能）
        Assert.Equal(0, best.RemainderMl);
        Assert.Equal(1000, best.TotalVolumeMl);
    }
}
