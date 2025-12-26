using TNTCalculatorRazor.Domain.Enums;

namespace TNTCalculatorRazor.Domain.Selectors;

public static class WeightForCalculationSelector
{
    public static double Select(
        WeightUsage usage,
        int age,
        double actualWeight,
        double adjustedWeight,
        double standardWeight,
        ProteinCondition proteinCondition )
    {
        switch (usage)
        {
            case WeightUsage.Energy:
                return age == 0
                    ? actualWeight
                    : adjustedWeight;

            case WeightUsage.Protein:
                {
                    // 乳児：必ず実測体重
                    if (age == 0)
                    {
                        return actualWeight;
                    }

                    // 成人のみ疾患別例外
                    if (proteinCondition is ProteinCondition.RenalFailure
                                         or ProteinCondition.Hemodialysis
                                         or ProteinCondition.LiverCirrhosis)
                    {
                        return standardWeight;
                    }

                    // 通常：調整体重
                    return adjustedWeight;
                }

            default:
                throw new ArgumentOutOfRangeException(nameof(usage));
        }
    }

}