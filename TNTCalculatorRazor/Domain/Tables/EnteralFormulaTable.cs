
using TNTCalculatorRazor.Domain.Enums;
using TNTCalculatorRazor.Domain.Models;

namespace TNTCalculatorRazor.Domain.Tables;

public static class EnteralFormulaTable
{
    private static readonly Dictionary<EnteralFormulaType, EnteralFormulaComposition> _table
        = new()
        {
            // kcalあたり: 容量, 蛋白, 脂質, 糖質, 食塩, VitK, 水分

            [EnteralFormulaType.Meibalance10] =
            new(1.0, 16.0 / 400, 11.2 / 400, 58.0 / 400, 1.12 / 400, 20.0 / 400, 338.0 / 400),

            [EnteralFormulaType.PeptamenPrebio15] =
            new(267.0 / 400, 14.0 / 400, 16.0 / 400, 50.0 / 400, 1.45 / 400, 33.0 / 400, 204.0 / 400),

            [EnteralFormulaType.PeptamenIntense10] =
            new(1.0, 18.4 / 200, 7.4 / 200, 15.0 / 200, 0.61 / 200, 24.0 / 200, 170.0 / 200),

            [EnteralFormulaType.PeptamenAF15] =
            new(200.0 / 300, 19.0 / 300, 13.2 / 300, 26.4 / 300, 0.61 / 300, 6.0 / 300, 155.0 / 300),

            [EnteralFormulaType.IsocalSupport15] =
            new(267.0 / 400, 15.2 / 400, 18.4 / 400, 40.9 / 400, 0.92 / 400, 46.8 / 400, 204.0 / 400),

            [EnteralFormulaType.Lacphia15] =
            new(267.0 / 400, 16.0 / 400, 12.0 / 400, 56.1 / 400, 1.21 / 400, 28.0 / 400, 206.0 / 400),

            [EnteralFormulaType.Mein10] =
            new(1.0, 10.0 / 200, 5.6 / 200, 26.2 / 200, 0.41 / 200, 4.6 / 200, 168.2 / 200),

            [EnteralFormulaType.RenalenMP16] =
            new(250.0 / 400, 14.0 / 400, 11.2 / 400, 60.0 / 400, 0.61 / 400, 5.6 / 400, 187.3 / 400),

            [EnteralFormulaType.GlucernaRex10] =
            new(1.0, 16.7 / 400, 22.3 / 400, 38.8 / 400, 0.96 / 400, 12.0 / 400, 340.0 / 400),

            [EnteralFormulaType.PGSoftEJ15] =
            new(267.0 / 400, 16.0 / 400, 8.8 / 400, 62.7 / 400, 1.38 / 400, 60.0 / 400, 175.0 / 400),

            [EnteralFormulaType.RacolNF10] =
            new(1.0, 8.76 / 200, 4.46 / 200, 31.24 / 200, 0.38 / 200, 12.5 / 200, 170.0 / 200),

            [EnteralFormulaType.RacolNFSemiSolid10] =
            new(1.0, 13.14 / 300, 6.69 / 300, 46.86 / 300, 0.57 / 300, 18.8 / 300, 228.0 / 300),

            [EnteralFormulaType.EnsureH15] =
            new(250.0 / 375, 13.2 / 375, 13.2 / 375, 51.5 / 375, 0.76 / 375, 26.3 / 375, 194.0 / 375),

            [EnteralFormulaType.Inoras16] =
            new(187.5 / 300, 12.0 / 300, 9.66 / 300, 39.79 / 300, 0.69 / 300, 24.99 / 300, 140.0 / 300),

            [EnteralFormulaType.Elental10] =
            new(1.0, 14.1 / 300, 0.51 / 300, 63.41 / 300, 0.66 / 300, 9.0 / 300, 250.0 / 300)
        };

    public static EnteralFormulaComposition Get( EnteralFormulaType type )
        => _table[type];
}
