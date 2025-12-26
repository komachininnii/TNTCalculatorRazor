namespace TNTCalculatorRazor.Domain.Calculators;

using TNTCalculatorRazor.Domain.Enums;
using TNTCalculatorRazor.Domain.Tables;

public static class ProteinCalculator
{
    public static double Calculate(
        int age,
        double weightForProtein,
        double stressFactor,
        double proteinCorrect )
    {
        double baseProtein =
            ProteinBaseCalculator.Calculate(age, weightForProtein);

        return baseProtein * stressFactor * proteinCorrect;
    }
}