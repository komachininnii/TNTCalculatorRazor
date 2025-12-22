namespace TNTCalculatorRazor.Domain
{
    public static class BmrCalculator
    {
        public static double Calculate(
            int age,
            double weightKg,
            double heightCm,
            Sex sex )
        {
            if (age < 0) throw new ArgumentOutOfRangeException(nameof(age));
            if (weightKg <= 0) throw new ArgumentOutOfRangeException(nameof(weightKg));
            if (heightCm <= 0) throw new ArgumentOutOfRangeException(nameof(heightCm));

            if (age == 0)
                return CalculateInfant(weightKg, sex);

            if (age <= 17)
                return CalculateChild(age, weightKg, sex);

            return CalculateAdult(age, weightKg, heightCm, sex);
        }


private static double CalculateInfant( double weight, Sex sex )
        {
            if (weight <= 10)
            {
                return (weight - 0.4) * 57;
            }

            return sex switch
            {
                Sex.Male => (weight + 8.6) * 30.5,
                Sex.Female => (weight + 8.6) * 30.0,
                _ => throw new ArgumentOutOfRangeException()
            };
        }


        private static double CalculateChild(
            int age,
            double weight,
            Sex sex )
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

            return coefficient * weight;
        }

        private static double CalculateAdult(
            int age,
            double weight,
            double height,
            Sex sex )
        {
            // Harris-Benedict 適応条件
            if (weight >= 25 && height >= 151)
            {
                return sex switch
                {
                    Sex.Male =>
                        66.47 + (13.75 * weight) + (5.0 * height) - (6.76 * age),

                    Sex.Female =>
                        655.1 + (9.56 * weight) + (1.85 * height) - (4.68 * age),

                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            return CalculateGanpule(age, weight, height, sex);
        }
        private static double CalculateGanpule(
            int age,
            double weight,
            double height,
            Sex sex )
        {
            double sexFactor = sex == Sex.Male ? 1.0 : 2.0;

            return (0.1238
                + (0.0481 * weight)
                + (0.0234 * height)
                - (0.0138 * age)
                - (0.5473 * sexFactor))
                * 1000.0 / 4.186;
        }
    }
}
