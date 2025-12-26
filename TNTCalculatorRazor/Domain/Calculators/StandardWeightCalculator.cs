using TNTCalculatorRazor.Domain.Enums;

namespace TNTCalculatorRazor.Domain.Calculators;

public static class StandardWeightCalculator
{
    /// <summary>
    /// 標準体重（Web Forms 完全互換）
    /// 小児：日本小児内分泌学会
    /// 成人：BMI = 22
    /// </summary>
    public static double Calculate(
        int age,
        double heightCm,
        Sex sex )
    {
        if (age < 6)
        {
            return CalculateInfant(heightCm, sex);
        }
        else if (age < 18)
        {
            return CalculateChild(heightCm, sex);
        }
        else
        {
            // 成人：BMI 22
            return heightCm * heightCm * 22.0 * 0.0001;
        }
    }

    private static double CalculateInfant( double height, Sex sex )
    {
        return sex == Sex.Male
            ? 0.00206 * Math.Pow(height, 2) - 0.1166 * height + 6.5273
            : 0.00249 * Math.Pow(height, 2) - 0.1858 * height + 9.036;
    }

    private static double CalculateChild( double height, Sex sex )
    {
        if (height < 140)
        {
            return sex == Sex.Male
                ? 0.0000303882 * Math.Pow(height, 3)
                  - 0.00571495 * Math.Pow(height, 2)
                  + 0.508124 * height
                  - 9.17791
                : 0.000127719 * Math.Pow(height, 3)
                  - 0.0414712 * Math.Pow(height, 2)
                  + 4.8575 * height
                  - 184.492;
        }
        else if (height < 149)
        {
            return sex == Sex.Male
                ? -8.5013E-05 * Math.Pow(height, 3)
                  + 0.0370692 * Math.Pow(height, 2)
                  - 4.6558 * height
                  + 191.847
                : -0.00178766 * Math.Pow(height, 3)
                  + 0.803922 * Math.Pow(height, 2)
                  - 119.31 * height
                  + 5885.03;
        }
        else
        {
            return sex == Sex.Male
                ? -0.000310205 * Math.Pow(height, 3)
                  + 0.151159 * Math.Pow(height, 2)
                  - 23.6303 * height
                  + 1231.04
                : 0.000956401 * Math.Pow(height, 3)
                  - 0.462755 * Math.Pow(height, 2)
                  + 75.3058 * height
                  - 4068.31;
        }
    }
}