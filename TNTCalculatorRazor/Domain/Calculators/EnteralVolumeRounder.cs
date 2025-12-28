using TNTCalculatorRazor.Domain.Enums;
using TNTCalculatorRazor.Domain.Tables;

namespace TNTCalculatorRazor.Domain.Calculators;

public static class EnteralVolumeRounder
{
    /// <summary>
    /// 規格量へ切り上げ丸め
    /// </summary>
    public static int RoundUp(
        EnteralFormulaType formula,
        double requestedVolume )
    {
        var sizes = EnteralPackageTable.Get(formula);

        // requestedVolume 以上の最小規格量
        foreach (var size in sizes.OrderBy(x => x))
        {
            if (requestedVolume <= size)
                return size;
        }

        // 全部超えた場合 → 最大規格
        return sizes.Max();
    }
}
