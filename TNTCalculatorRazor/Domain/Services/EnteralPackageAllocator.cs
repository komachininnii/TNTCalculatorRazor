using System;
using System.Collections.Generic;
using System.Linq;

namespace TNTCalculatorRazor.Domain.Services
{
    /// <summary>
    /// 1日必要投与量（mL）を、製剤規格（1本あたりmL）で割り付けるサービス
    /// ・必要量を超えない範囲で割付
    /// ・余り（端数）を最小化
    /// ・成分計算は行わない
    /// </summary>
    public static class EnteralPackageAllocator
    {
        public static IReadOnlyList<EnteralPackagePlan> BuildPlans(
    int targetVolumeMl,
    IReadOnlyList<int> packageVolumes,
    int maxPlans = 5 )
        {
            if (targetVolumeMl <= 0) return Array.Empty<EnteralPackagePlan>();

            var vols = packageVolumes
                .Where(v => v > 0)
                .Distinct()
                .OrderByDescending(v => v)
                .ToArray();

            if (vols.Length == 0) return Array.Empty<EnteralPackagePlan>();

            var minVol = vols.Min();
            var biggest = vols[0];

            var plans = new List<EnteralPackagePlan>();

            // 最大規格の本数を振って候補を作る
            var maxBigCount = targetVolumeMl / biggest;

            for (int bigCount = maxBigCount; bigCount >= 0; bigCount--)
            {
                var counts = new Dictionary<int, int>();
                var used = 0;

                if (bigCount > 0)
                {
                    counts[biggest] = bigCount;
                    used += bigCount * biggest;
                }

                var remaining = targetVolumeMl - used;

                // 次に大きい規格から順に埋める
                for (int i = 1; i < vols.Length; i++)
                {
                    var v = vols[i];
                    var c = remaining / v;
                    if (c > 0)
                    {
                        counts[v] = c;
                        used += c * v;
                        remaining = targetVolumeMl - used;
                    }
                }

                // ★重要：最小規格で残りを可能な限り埋める（remainder < minVol を保証）
                if (remaining >= minVol)
                {
                    var add = remaining / minVol;
                    if (add > 0)
                    {
                        counts[minVol] = (counts.TryGetValue(minVol, out var cur) ? cur : 0) + add;
                        used += add * minVol;
                        remaining = targetVolumeMl - used;
                    }
                }

                plans.Add(new EnteralPackagePlan(counts, used, remaining));
            }

            // 重複（同じ内訳）を除去
            plans = plans
                .GroupBy(p => NormalizeKey(p))
                .Select(g => g.First())
                .ToList();

            return plans
                .OrderBy(p => p.RemainderMl)
                .ThenBy(p => p.TotalPackageCount)
                .Take(maxPlans)
                .ToList();

            static string NormalizeKey( EnteralPackagePlan p )
                => string.Join("|", p.CountsByVolume.OrderByDescending(k => k.Key)
                      .Select(kv => $"{kv.Key}x{kv.Value}")) + $":r{p.RemainderMl}";
        }

    }
}