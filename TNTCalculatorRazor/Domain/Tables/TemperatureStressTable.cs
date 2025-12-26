using TNTCalculatorRazor.Domain.Enums;

namespace TNTCalculatorRazor.Domain.Tables;

public static class TemperatureStressTable
{
    public static double GetAddition( BodyTemperatureLevel level )
    {
        return level switch
        {
            BodyTemperatureLevel.Fever37 => 0.2,
            BodyTemperatureLevel.Fever38 => 0.4,
            BodyTemperatureLevel.Fever39 => 0.6,
            BodyTemperatureLevel.Fever40 => 0.8,
            _ => 0.0
        };
    }
}
