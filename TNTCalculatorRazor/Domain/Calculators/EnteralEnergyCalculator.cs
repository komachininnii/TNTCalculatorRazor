using TNTCalculatorRazor.Domain.Models;

namespace TNTCalculatorRazor.Domain.Calculators;

public static class EnteralEnergyCalculator
{
    public static double CalculateEnergyFromVolume(
        double volume,
        EnteralFormulaComposition comp )
    {
        return volume / comp.VolumePerKcal;
    }
}
