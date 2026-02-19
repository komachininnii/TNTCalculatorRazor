using TNTCalculatorRazor.Domain.Models;

namespace TNTCalculatorRazor.Domain.Calculators;

public static class EnteralEnergyCalculator
{
    public static double CalculateEnergyFromVolume(
        double volume,
        EnteralFormulaComposition comp )
    {
        if (volume <= 0) return 0.0;
        if (comp is null) throw new ArgumentNullException(nameof(comp));
        if (comp.VolumePerKcal <= 0) throw new ArgumentOutOfRangeException(nameof(comp.VolumePerKcal));

        return volume / comp.VolumePerKcal;
    }
}
