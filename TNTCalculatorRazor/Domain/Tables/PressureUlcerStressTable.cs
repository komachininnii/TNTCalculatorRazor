using TNTCalculatorRazor.Domain.Enums;

namespace TNTCalculatorRazor.Domain.Tables;

public static class PressureUlcerStressTable
{
    public static double GetAddition( PressureUlcerLevel level )
    {
        return level switch
        {
            PressureUlcerLevel.D1ToD2 => 0.1,
            PressureUlcerLevel.D3 => 0.2,
            PressureUlcerLevel.D4 => 0.3,
            PressureUlcerLevel.D5 => 0.4,
            _ => 0.0
        };
    }
}
