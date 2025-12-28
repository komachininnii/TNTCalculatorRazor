using TNTCalculatorRazor.Domain.Enums;
using TNTCalculatorRazor.Domain.Models;
using TNTCalculatorRazor.Domain.Tables;

namespace TNTCalculatorRazor.Domain.Services;

public static class EnteralDoseCalculator
{
    //========================================
    // kcal → mL
    //========================================
    public static EnteralDoseResult FromEnergy(
        double targetKcal,
        EnteralFormulaType formula )
    {
        var comp = EnteralFormulaTable.Get(formula);

        double volume =
            targetKcal * comp.VolumePerKcal;

        return new EnteralDoseResult
        {
            EnergyKcal = targetKcal,
            VolumeMl = volume,
            VolumePerKcal = comp.VolumePerKcal
        };
    }

    //========================================
    // mL → kcal（手入力）
    //========================================
    public static EnteralDoseResult FromVolume(
        double volumeMl,
        EnteralFormulaType formula )
    {
        var comp = EnteralFormulaTable.Get(formula);

        double kcal =
            volumeMl / comp.VolumePerKcal;

        return new EnteralDoseResult
        {
            EnergyKcal = kcal,
            VolumeMl = volumeMl,
            VolumePerKcal = comp.VolumePerKcal
        };
    }

    //========================================
    // 規格に合わせて丸める（例：200 / 300 / 400 mL）
    //========================================
    public static double RoundToPackage(
        double volumeMl,
        EnteralPackageSize packageSize )
    {
        double size = (int)packageSize;

        return Math.Round(
            volumeMl / size,
            MidpointRounding.AwayFromZero)
            * size;
    }
}
