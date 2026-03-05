
using System;
using System.Collections.Generic;
using TNTCalculatorRazor.Domain.Enums;
using TNTCalculatorRazor.Domain.Models;

namespace TNTCalculatorRazor.Domain.Tables;

public static class EnteralFormulaTable
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
    private static readonly Dictionary<EnteralFormulaType, EnteralFormulaComposition> _table
        = new()
        {
            // PerPack(packKcal, Volume(mL), 蛋白質(g), 脂質(g), 糖質(g), 食塩(g), VitK(µg), 水分(mL))

            [EnteralFormulaType.Meibalance10] =
                PerPack(400, 400, 16.0, 11.2, 58.8, 1.12, 20.0, 337.2),     // メイバランス1.0

            [EnteralFormulaType.PeptamenPrebio15] =
                PerPack(400, 267, 15.2, 17.2, 44.4, 1.65, 33.0, 204),       // ペプタメンプレビオ1.5

            [EnteralFormulaType.PeptamenIntense10] =
                PerPack(200, 200, 18.4, 7.4, 15.0, 0.76, 24.0, 170),        // ペプタメンインテンス1.0

            [EnteralFormulaType.PeptamenAF15] =
                PerPack(300, 200, 19.0, 13.2, 26.4, 1.34, 26.0, 155),       // ペプタメンAF1.5

            [EnteralFormulaType.IsocalSupport15] =
                PerPack(400, 267, 15.2, 18.4, 40.8, 1.68, 49.0, 204),       // アイソカルサポート1.5

            [EnteralFormulaType.Lacphia15] =
                PerPack(400, 267, 16.0, 12.0, 56.0, 1.22, 28.0, 205),       // ラクフィア1.5

            [EnteralFormulaType.Mein10] =
                PerPack(200, 200, 10.0, 5.6, 26.4, 0.41, 2.8, 168.2),       // MEIN1.0

            [EnteralFormulaType.RenalenMP16] =
                PerPack(400, 250, 14.0, 11.2, 59.2, 0.61, 5.6, 188.8),      // リーナレンMP1.6

            [EnteralFormulaType.GlucernaRex10] =
                PerPack(400, 400, 16.7, 22.3, 35.2, 0.955, 12.0, 340),      // グルセナRex1.0

            [EnteralFormulaType.PGSoftEJ15] =
                PerPack(400, 267, 16.0, 8.8, 62.7, 1.38, 60.0, 175),        // PGソフトEJ1.5

            [EnteralFormulaType.RacolNF10] =
                PerPack(200, 200, 8.76, 4.46, 31.24, 0.38, 12.5, 170),      // ラコールNF1.0

            [EnteralFormulaType.RacolNFSemiSolid10] =
                PerPack(300, 300, 13.14, 6.69, 46.86, 0.57, 18.75, 228),    // ラコールNF半固形1.0

            [EnteralFormulaType.EnsureH15] =
                PerPack(375, 250, 13.2, 13.2, 51.5, 0.76, 26.3, 194),       // エンシュアH1.5

            [EnteralFormulaType.Inoras16] =
                PerPack(300, 187.5, 12.0, 9.66, 39.79, 0.69, 24.99, 140),   // イノラス1.6

            [EnteralFormulaType.Elental10] =
                PerPack(300, 300, 14.1, 0.51, 63.41, 0.66, 9.0, 250),       // エレンタール1.0
        };

    public static EnteralFormulaComposition Get( EnteralFormulaType type )
    {
        if (!_table.TryGetValue(type, out var c))
            throw new InvalidOperationException(
                $"EnteralFormulaTable に未登録の製剤です: {type}");

        return c;
    }
}
