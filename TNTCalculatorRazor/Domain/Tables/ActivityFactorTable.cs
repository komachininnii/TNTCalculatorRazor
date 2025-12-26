using TNTCalculatorRazor.Domain.Enums;

namespace TNTCalculatorRazor.Domain.Tables;

public static class ActivityFactorTable
{
    public static double Get( ActivityFactorType type )
    {
        return type switch
        {
            ActivityFactorType.BedriddenComa    => 1.0,
            ActivityFactorType.BedriddenAwake   => 1.1,
            ActivityFactorType.Sitting          => 1.2,
            ActivityFactorType.Wheelchair       => 1.3,
            ActivityFactorType.Walking          => 1.4,
            ActivityFactorType.Rehabilitation15 => 1.5,
            ActivityFactorType.Rehabilitation16 => 1.6,
            ActivityFactorType.Rehabilitation17 => 1.7,
            _ => 1.0
        };
    }
}