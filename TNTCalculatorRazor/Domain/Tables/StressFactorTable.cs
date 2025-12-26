using TNTCalculatorRazor.Domain.Enums;

namespace TNTCalculatorRazor.Domain.Tables;

public static class StressFactorTable
{
    public static double Get( StressFactorType type ) =>
        type switch
        {
            StressFactorType.Normal => 1.0,
            StressFactorType.MildStress => 1.1,

            StressFactorType.InfectMild => 1.2,
            StressFactorType.InfectModerate => 1.5,
            StressFactorType.InfectSevere => 1.8,

            StressFactorType.SurgeryMinor => 1.2,
            StressFactorType.SurgeryModerate => 1.4,
            StressFactorType.SurgeryMajor => 1.6,
            StressFactorType.SurgeryVeryMajor => 1.8,

            StressFactorType.TraumaBone => 1.35,
            StressFactorType.TraumaMuscle => 1.3,
            StressFactorType.TraumaHeadSteroid => 1.6,

            StressFactorType.Burn20 => 1.5,
            StressFactorType.Burn40 => 1.85,
            StressFactorType.Burn100 => 2.05,

            _ => 1.0
        };
}