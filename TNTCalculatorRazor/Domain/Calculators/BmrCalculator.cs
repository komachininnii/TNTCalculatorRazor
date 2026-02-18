using TNTCalculatorRazor.Domain.Enums;
using TNTCalculatorRazor.Domain.Results;

namespace TNTCalculatorRazor.Domain.Calculators;

public static class BmrCalculator
{
    public static BmrResult Calculate(
        int age,
        double weightKg,
        double heightCm,
        GenderType gender )
    {
        if (age < 0) throw new ArgumentOutOfRangeException(nameof(age));

        if (age == 0)
            return CalculateInfant(weightKg, gender);

        if (age <= 17)
            return CalculateChild(age, weightKg, gender);

        return CalculateAdult(age, weightKg, heightCm, gender);
    }


    private static BmrResult CalculateInfant(double weight, GenderType gender)
    {
        double raw;

        if (weight <= 10)
        {
            raw = (weight - 0.4) * 57;
        }
        else
        {
            raw = gender == GenderType.Male
                ? (weight + 8.6) * 30.5
                : (weight + 8.6) * 30.0;
        }

        return new BmrResult
        {
            RawValue = raw,
            Formula = BmrFormulaType.Infant_KyotoPICU
        };
    }

    // 小児：Schofield(1985) 体重ベース（kcal/day）
    // 境界：age < 3 / 3 <= age < 10 / 10 <= age < 18
    private static BmrResult CalculateChild( int age, double weight, GenderType gender )
    {
        double raw = age switch
        {
            < 3 => gender == GenderType.Male
                ? (59.512 * weight - 30.4)
                : (58.317 * weight - 31.1),

            < 10 => gender == GenderType.Male
                ? (22.706 * weight + 504.3)
                : (20.315 * weight + 485.9),

            _ => gender == GenderType.Male
                ? (17.686 * weight + 658.2)
                : (13.384 * weight + 692.6),
        };

        return new BmrResult
        {
            RawValue = raw,
            Formula = BmrFormulaType.Child_Schofield1985
        };
    }

    private static BmrResult CalculateAdult(
        int age,
        double weight,
        double height,
        GenderType gender)
    {
        if (weight >= 25 && height >= 151)
        {
            double raw = gender == GenderType.Male
                ? 66.47 + (13.75 * weight) + (5.0 * height) - (6.76 * age)
                : 655.1 + (9.56 * weight) + (1.85 * height) - (4.68 * age);

            return new BmrResult
            {
                RawValue = raw,
                Formula = BmrFormulaType.Adult_HarrisBenedict
            };
        }

        return CalculateGanpule(age, weight, height, gender);
    }
    private static BmrResult CalculateGanpule(
        int age,
        double weight,
        double height,
        GenderType gender )
    {
        double sexFactor = gender == GenderType.Male ? 1.0 : 2.0;

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
