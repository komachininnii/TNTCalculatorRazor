using System;
using System.Collections.Generic;
using System.Linq;
using TNTCalculatorRazor.Domain.Enums;
using TNTCalculatorRazor.Domain.Models;

namespace TNTCalculatorRazor.Domain.Tables;

public static class EnteralFormulaData
{
    /// <summary>
    /// 添付文書の「○○kcal あたり」をそのまま書けるようにするヘルパー。
    /// 内部では "per kcal"（mL/kcal, g/kcal, …）へ正規化して保持する。
    ///
    /// 引数順:
    ///   (packKcal, volumeMl, proteinG, fatG, carbG, saltG, vitKug, waterMl)
    /// </summary>
    private static EnteralFormulaComposition PerPack(
        double packKcal,
        double volumeMl,
        double proteinG,
        double fatG,
        double carbG,
        double saltG,
        double vitKug,
        double waterMl )
        => new(
            volumeMl / packKcal,
            proteinG / packKcal,
            fatG / packKcal,
            carbG / packKcal,
            saltG / packKcal,
            vitKug / packKcal,
            waterMl / packKcal
        );

    // 2026/03/05 テーブル更新（メーカーサイト参照）
    private static readonly Dictionary<EnteralFormulaType, EnteralFormulaInfo> _table
        = new()
        {
            // PerPack(packKcal, Volume(mL), 蛋白質(g), 脂質(g), 糖質(g), 食塩(g), VitK(µg), 水分(mL))

            [EnteralFormulaType.Meibalance10] =
                new(
                    PerPack(400, 400, 16.0, 11.2, 58.8, 1.12, 20.0, 337.2),
                    new[] { 300, 400 }),

            [EnteralFormulaType.PeptamenPrebio15] =
                new(
                    PerPack(400, 267, 15.2, 17.2, 44.4, 1.65, 33.0, 204),
                    new[] { 200, 267 }),

            [EnteralFormulaType.PeptamenIntense10] =
                new(
                    PerPack(200, 200, 18.4, 7.4, 15.0, 0.76, 24.0, 170),
                    new[] { 200 }),

            [EnteralFormulaType.PeptamenAF15] =
                new(
                    PerPack(300, 200, 19.0, 13.2, 26.4, 1.34, 26.0, 155),
                    new[] { 200 }),

            [EnteralFormulaType.IsocalSupport15] =
                new(
                    PerPack(400, 267, 15.2, 18.4, 40.8, 1.68, 49.0, 204),
                    new[] { 200, 267 }),

            [EnteralFormulaType.Lacphia15] =
                new(
                    PerPack(400, 267, 16.0, 12.0, 56.0, 1.22, 28.0, 205),
                    new[] { 200, 267 }),

            [EnteralFormulaType.Mein10] =
                new(
                    PerPack(200, 200, 10.0, 5.6, 26.4, 0.41, 2.8, 168.2),
                    new[] { 200 }),

            [EnteralFormulaType.RenalenMP16] =
                new(
                    PerPack(400, 250, 14.0, 11.2, 59.2, 0.61, 5.6, 188.8),
                    new[] { 250 }),

            [EnteralFormulaType.GlucernaRex10] =
                new(
                    PerPack(400, 400, 16.7, 22.3, 35.2, 0.955, 12.0, 340),
                    new[] { 200, 400 }),

            [EnteralFormulaType.PGSoftEJ15] =
                new(
                    PerPack(400, 267, 16.0, 8.8, 62.7, 1.38, 60.0, 175),
                    new[] { 200, 267 }),

            [EnteralFormulaType.RacolNF10] =
                new(
                    PerPack(200, 200, 8.76, 4.46, 31.24, 0.38, 12.5, 170),
                    new[] { 200 }),

            [EnteralFormulaType.RacolNFSemiSolid10] =
                new(
                    PerPack(300, 300, 13.14, 6.69, 46.86, 0.57, 18.75, 228),
                    new[] { 300 }),

            [EnteralFormulaType.EnsureH15] =
                new(
                    PerPack(375, 250, 13.2, 13.2, 51.5, 0.76, 26.3, 194),
                    new[] { 250 }),

            [EnteralFormulaType.Inoras16] =
                new(
                    PerPack(300, 187.5, 12.0, 9.66, 39.79, 0.69, 24.99, 140),
                    new[] { 187 }), // 実規格は187.5mLだが、旧WebForms互換のため187で固定

            [EnteralFormulaType.Elental10] =
                new(
                    PerPack(300, 300, 14.1, 0.51, 63.41, 0.66, 9.0, 250),
                    new[] { 300 })
        };

    public static EnteralFormulaInfo Get( EnteralFormulaType type )
    {
        if (!_table.TryGetValue(type, out var info))
            throw new InvalidOperationException(
                $"EnteralFormulaData に未登録の製剤です: {type}");

        return info;
    }

    public static EnteralFormulaComposition GetComposition( EnteralFormulaType type )
        => Get(type).Composition;

    public static IReadOnlyList<int> GetPackages( EnteralFormulaType type )
        => Get(type).Packages.OrderBy(x => x).ToArray();
}
