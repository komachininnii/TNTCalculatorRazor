namespace TNTCalculatorRazor.Domain.Calculators;

public static class ProteinBaseCalculator
{
    public static double Calculate( int age, double weight )
    {
        if (age == 0) return 2.0 * weight;
        else if (age <= 3) return 1.8 * weight;
        else if (age <= 6) return 1.5 * weight;
        else if (age <= 10) return 1.2 * weight;
        else return 1.0 * weight;
    }
}