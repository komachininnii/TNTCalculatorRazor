using TNTCalculatorRazor.Domain.Enums;

namespace TNTCalculatorRazor.Domain.Tables;

public static class EnteralPackageTable
{
    private static readonly Dictionary<EnteralFormulaType, int[]> _table
        = new()
        {
            [EnteralFormulaType.Meibalance10] =
                new[] { 300, 400 },

            [EnteralFormulaType.PeptamenPrebio15] =
                new[] { 200, 267 },

            [EnteralFormulaType.PeptamenIntense10] =
                new[] { 200 },

            [EnteralFormulaType.PeptamenAF15] =
                new[] { 200 },

            [EnteralFormulaType.IsocalSupport15] =
                new[] { 200, 267 },

            [EnteralFormulaType.Lacphia15] =
                new[] { 200, 267 },

            [EnteralFormulaType.Mein10] =
                new[] { 200 },

            [EnteralFormulaType.RenalenMP16] =
                new[] { 250 },

            [EnteralFormulaType.GlucernaRex10] =
                new[] { 200, 400 },

            [EnteralFormulaType.PGSoftEJ15] =
                new[] { 200, 267 },

            [EnteralFormulaType.RacolNF10] =
                new[] { 200 },

            [EnteralFormulaType.RacolNFSemiSolid10] =
                new[] { 300 },

            [EnteralFormulaType.EnsureH15] =
                new[] { 250 },

            [EnteralFormulaType.Inoras16] =
                new[] { 187 },  // 実規格は187.5mLだが、旧WebForms互換のため187で固定

            [EnteralFormulaType.Elental10] =
                new[] { 300 }
        };

    public static IReadOnlyList<int> Get( EnteralFormulaType type )
    {
        if (!_table.TryGetValue(type, out var volumes))
            throw new InvalidOperationException(
                $"EnteralPackageTable に未登録の製剤です: {type}");

        // 念のため昇順保証（RoundUp前提）
        return volumes.OrderBy(x => x).ToArray();
    }
}