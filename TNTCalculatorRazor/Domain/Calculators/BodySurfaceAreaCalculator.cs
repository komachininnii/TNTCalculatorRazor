namespace TNTCalculatorRazor.Domain.Calculators
{
    /// <summary>
    /// 体表面積（Body Surface Area）
    /// DuBois式
    /// </summary>
    public static class BodySurfaceAreaCalculator
    {
        /// <summary>
        /// 体表面積を算出する（m²）
        /// </summary>
        public static double Calculate(
            double heightCm,
            double weightKg )
        {
            return 71.84
                   * Math.Pow(heightCm, 0.725)
                   * Math.Pow(weightKg, 0.425)
                   * 0.0001;
        }
    }
}