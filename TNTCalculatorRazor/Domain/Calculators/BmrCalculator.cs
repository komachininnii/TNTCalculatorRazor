using TNTCalculatorRazor.Domain.Enums;
using TNTCalculatorRazor.Domain.Results;

namespace TNTCalculatorRazor.Domain.Calculators;

public static class BmrCalculator
{
    public static BmrResult Calculate(
        int age,
        double weightKg,
        double heightCm,
        Sex sex )
    {
        if (age == 0)
            return CalculateInfant(weightKg, sex);

        if (age <= 17)
            return CalculateChild(age, weightKg, sex);

        return CalculateAdult(age, weightKg, heightCm, sex);
    }


    private static BmrResult CalculateInfant( double weight, Sex sex )
    {
        double raw;

        if (weight <= 10)
        {
            raw = (weight - 0.4) * 57;
        }
        else
        {
            raw = sex == Sex.Male
                ? (weight + 8.6) * 30.5
                : (weight + 8.6) * 30.0;
        }

        return new BmrResult
        {
            RawValue = raw,
            Formula = BmrFormulaType.Infant_KyotoPICU
        };
    }


    private static BmrResult CalculateChild( int age, double weight, Sex sex )
    {
        double coefficient = age switch
        {
            <= 2 => sex == Sex.Male ? 61.0 : 59.7,
            <= 5 => sex == Sex.Male ? 54.8 : 52.2,
            <= 7 => sex == Sex.Male ? 44.3 : 41.9,
            <= 9 => sex == Sex.Male ? 40.8 : 38.3,
            <= 11 => sex == Sex.Male ? 37.4 : 34.8,
            <= 14 => sex == Sex.Male ? 31.0 : 29.6,
            <= 17 => sex == Sex.Male ? 27.0 : 25.3,
            _ => throw new ArgumentOutOfRangeException()
        };

        return new BmrResult
        {
            RawValue = coefficient * weight,
            Formula = BmrFormulaType.Child_JapanDRI2010
        };
    }

    private static BmrResult CalculateAdult(
        int age,
        double weight,
        double height,
        Sex sex )
    {
        if (weight >= 25 && height >= 151)
        {
            double raw = sex == Sex.Male
                ? 66.47 + (13.75 * weight) + (5.0 * height) - (6.76 * age)
                : 655.1 + (9.56 * weight) + (1.85 * height) - (4.68 * age);

            return new BmrResult
            {
                RawValue = raw,
                Formula = BmrFormulaType.Adult_HarrisBenedict
            };
        }

        return CalculateGanpule(age, weight, height, sex);
    }
    private static BmrResult CalculateGanpule(
        int age,
        double weight,
        double height,
    Sex sex )
    {
        double sexFactor = sex == Sex.Male ? 1.0 : 2.0;

        double raw =
            (0.1238
            + (0.0481 * weight)
            + (0.0234 * height)
            - (0.0138 * age)
            - (0.5473 * sexFactor))
            * 1000.0 / 4.186;

        return new BmrResult
        {
            RawValue = raw,
            Formula = BmrFormulaType.Adult_Ganpule2007
        };
    }
}
