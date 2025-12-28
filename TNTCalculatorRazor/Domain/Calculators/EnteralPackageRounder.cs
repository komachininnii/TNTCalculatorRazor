using TNTCalculatorRazor.Domain.Enums;

namespace TNTCalculatorRazor.Domain.Calculators;

public static class EnteralPackageRounder
{
    public static int RoundUp(
        double rawVolume,
        IEnumerable<int> packageVolumes )
    {
        var list = packageVolumes?.ToList() ?? new List<int>();
        if (list.Count == 0)
        {
            // 候補がない場合は「生の量を切り上げ」程度で返す（落とさない）
            return (int)Math.Ceiling(rawVolume);
        }

        return list
            .Where(v => v >= rawVolume)
            .DefaultIfEmpty(list.Max())
            .Min();
    }

    public static int ApplySelected( int selectedPackageVolume )
        => selectedPackageVolume;
}

