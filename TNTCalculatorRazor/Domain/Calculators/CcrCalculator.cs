using TNTCalculatorRazor.Domain.Enums;

namespace TNTCalculatorRazor.Domain.Calculators;

public static class CcrCalculator
{
    // Cockcroft–Gault
    public static double Calculate( int age, double weightKg, double creatinineMgDl, GenderType gender )
    {
        if (creatinineMgDl <= 0) return 0;

        double baseVal = ((140.0 - age) * weightKg) / (72.0 * creatinineMgDl);
        return gender == GenderType.Female ? baseVal * 0.85 : baseVal;
    }
}
