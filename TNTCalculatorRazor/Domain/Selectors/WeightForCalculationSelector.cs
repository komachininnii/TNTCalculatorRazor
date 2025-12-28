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
    DiseaseType disease )
    {
        switch (usage)
        {
            case WeightUsage.Energy:
                // 乳児は実測体重、それ以外は調整体重
                return age == 0
                    ? actualWeight
                    : adjustedWeight;

            case WeightUsage.Protein:
                {
                    // ★ 小児（0～17歳）は常に実測体重
                    if (age < 18)
                    {
                        return actualWeight;
                    }

                    // ★ 成人：疾患別例外では標準体重
                    if (disease is DiseaseType.RenalFailure
                                or DiseaseType.Hemodialysis
                                or DiseaseType.LiverCirrhosis)
                    {
                        return standardWeight;
                    }

                    // ★ 成人・通常
                    return adjustedWeight;
                }

            default:
                throw new ArgumentOutOfRangeException(nameof(usage));
        }
    }


}