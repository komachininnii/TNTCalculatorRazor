using TNTCalculatorRazor.Domain.Enums;
using TNTCalculatorRazor.Domain.Rules;

namespace TNTCalculatorRazor.Domain.Selectors;

public static class WeightForCalculationSelector
{
    public static double Select(
        WeightUsage usage,
        int age,
        double actualWeight,
        double correctedWeight,
        double standardWeight,
        DiseaseType disease )
    {
        switch (usage)
        {
            case WeightUsage.Energy:
                // 年齢0歳（乳児）は実測体重、それ以外は CorrectedWeight（最終採用体重）
                return age == 0
                    ? actualWeight 
                    : correctedWeight;

            case WeightUsage.Protein:
                {
                    // 小児（0～17歳）は常に実測体重
                    if (age < 18) return actualWeight;

                    // 成人：例外疾患では標準体重
                    if (ProteinRule.UseStandardWeightForProtein(age, disease))
                        return standardWeight;

                    // 成人・通常は補正体重
                    return correctedWeight;
                }

            default:
                throw new ArgumentOutOfRangeException(nameof(usage));
        }
    }
}
