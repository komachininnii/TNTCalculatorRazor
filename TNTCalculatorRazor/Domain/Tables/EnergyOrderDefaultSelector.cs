
using TNTCalculatorRazor.Domain.Enums;

namespace TNTCalculatorRazor.Domain.Tables;

public static class EnergyOrderDefaultSelector
{
    public static EnergyOrderType GetDefault(DiseaseType disease)
        => disease switch
        {
            DiseaseType.None => EnergyOrderType.BmrEstimated,
            DiseaseType.Diabetes => EnergyOrderType.Kcal25,
            DiseaseType.RenalFailure => EnergyOrderType.Kcal30,
            DiseaseType.Hemodialysis => EnergyOrderType.Kcal30,
            DiseaseType.LiverCirrhosis => EnergyOrderType.Kcal35,
            _ => EnergyOrderType.BmrEstimated
        };
}
