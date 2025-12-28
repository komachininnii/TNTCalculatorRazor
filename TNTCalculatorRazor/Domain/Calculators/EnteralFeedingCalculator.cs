using TNTCalculatorRazor.Domain.Models;

namespace TNTCalculatorRazor.Domain.Calculators;

public static class EnteralFeedingCalculator
{
    /// <summary>
    /// 投与カロリー → 投与量(mL)
    /// </summary>
    public static double CalculateVolumeMl(
        double targetKcal,
        EnteralFormulaComposition composition )
    {
        return targetKcal * composition.VolumePerKcal;
    }

    /// <summary>
    /// 投与量(mL) → 実投与カロリー
    /// </summary>
    public static double CalculateKcal(
        double volumeMl,
        EnteralFormulaComposition composition )
    {
        return volumeMl / composition.VolumePerKcal;
    }

    /// <summary>
    /// 成分量計算
    /// </summary>
    public static EnteralFeedingResult CalculateComponents(
        double targetKcal,
        EnteralFormulaComposition c )
    {
        return new EnteralFeedingResult
        {
            EnergyKcal = targetKcal,
            VolumeMl = targetKcal * c.VolumePerKcal,

            ProteinG = targetKcal * c.ProteinPerKcal,
            FatG = targetKcal * c.FatPerKcal,
            CarbG = targetKcal * c.CarbPerKcal,
            SaltG = targetKcal * c.SaltPerKcal,
            VitaminKUg = targetKcal * c.VitaminKPerKcal,
            WaterMl = targetKcal * c.WaterPerKcal
        };
    }
}
