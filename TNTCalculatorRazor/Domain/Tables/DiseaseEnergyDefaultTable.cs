using TNTCalculatorRazor.Domain.Enums;

namespace TNTCalculatorRazor.Domain.Tables;

public static class DiseaseEnergyDefaultTable
{
    public static EnergyOrderType Get( DiseaseType disease )
        => disease switch
        {
            DiseaseType.Diabetes => EnergyOrderType.Kcal25,
            DiseaseType.RenalFailure => EnergyOrderType.Kcal30,
            DiseaseType.Hemodialysis => EnergyOrderType.Kcal30,
            DiseaseType.LiverCirrhosis => EnergyOrderType.Kcal35,
            _ => EnergyOrderType.BmrEstimated
        };
}