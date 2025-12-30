namespace TNTCalculatorRazor.Domain.Calculators;

using TNTCalculatorRazor.Domain.Enums;
using TNTCalculatorRazor.Domain.Rules;

public static class ProteinCalculator
{
    public static double Calculate(
        int age,
        double weightForProtein,
        double stressFactor,
        double proteinCorrect,
        DiseaseType disease )
    {
        double baseProtein =
            ProteinBaseCalculator.Calculate(age, weightForProtein);

        // 例外疾患ではストレス係数を掛けない
        double appliedStress =
            ProteinRule.IsStressFactorIgnored(age, disease)
                ? 1.0
                : stressFactor;

        return baseProtein * appliedStress * proteinCorrect;
    }
}
