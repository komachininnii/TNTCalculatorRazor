using TNTCalculatorRazor.Domain.Enums;

namespace TNTCalculatorRazor.Domain.Tables;

public static class EnergyKcalPerKgTable
{
    public static double Get( EnergyOrderType type )
        => type switch
        {
            EnergyOrderType.Kcal25 => 25.0,
            EnergyOrderType.Kcal30 => 30.0,
            EnergyOrderType.Kcal35 => 35.0,
            _ => 0.0
        };
}