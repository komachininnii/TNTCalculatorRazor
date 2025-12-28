using TNTCalculatorRazor.Domain.Enums;

namespace TNTCalculatorRazor.Domain.Tables;

public static class EnergyOrderValueTable
{
    public static double Calculate(
        EnergyOrderType type,
        double bmrEnergy,
        double standardWeight )
    {
        return type switch
        {
            EnergyOrderType.BmrEstimated => bmrEnergy,
            EnergyOrderType.Kcal25 => 25 * standardWeight,
            EnergyOrderType.Kcal30 => 30 * standardWeight,
            EnergyOrderType.Kcal35 => 35 * standardWeight,
            _ => bmrEnergy
        };
    }
}

