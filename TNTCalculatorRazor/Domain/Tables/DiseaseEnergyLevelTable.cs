using TNTCalculatorRazor.Domain.Enums;

namespace TNTCalculatorRazor.Domain.Tables;

public static class DiseaseEnergyLevelTable
{
    public static EnergyOrderType GetDefault( DiseaseType disease )
    {
        return disease switch
        {
            DiseaseType.Diabetes => EnergyOrderType.Kcal30,
            DiseaseType.RenalFailure => EnergyOrderType.Kcal25,
            DiseaseType.Hemodialysis => EnergyOrderType.Kcal35,
            DiseaseType.LiverCirrhosis => EnergyOrderType.Kcal35,
            _ => EnergyOrderType.Kcal30
        };
    }
}