using TNTCalculatorRazor.Domain.Enums;
using TNTCalculatorRazor.Domain.Results;

namespace TNTCalculatorRazor.Domain.Calculators;

public static class BodyIndexCalculator
{
    /// <summary>
    /// BMI・標準体重・肥満度を計算
    /// </summary>
    public static BodyIndexResult Calculate(
     int age,
     double heightCm,
     double weightKg,
     Sex sex )
    {
        double heightM = heightCm / 100.0;

        double bmi = weightKg / (heightM * heightM);

        double standardWeight =
            StandardWeightCalculator.Calculate(age, heightCm, sex);

        double? obesityDegree = null;

        if (age > 0)
        {
            obesityDegree = weightKg / standardWeight * 100.0;
        }

        return new BodyIndexResult
        {
            Bmi = bmi,
            StandardWeight = standardWeight,
            ObesityDegree = obesityDegree
        };
    }
}