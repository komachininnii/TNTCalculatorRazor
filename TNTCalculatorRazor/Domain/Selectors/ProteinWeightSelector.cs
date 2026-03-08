using TNTCalculatorRazor.Domain.Enums;
using TNTCalculatorRazor.Domain.Rules;

namespace TNTCalculatorRazor.Domain.Selectors;

public static class ProteinWeightSelector
{
    public static double Select(
        int age,
        double actualWeight,
        double correctedWeight,
        double standardWeight,
        DiseaseType disease )
    {
        // 小児（0～17歳）は常に実測体重
        if (age < 18) return actualWeight;

        // 成人：例外疾患では標準体重
        if (ProteinRule.UseStandardWeightForProtein(age, disease))
            return standardWeight;

        // 成人・通常は補正体重
        return correctedWeight;
    }
}
