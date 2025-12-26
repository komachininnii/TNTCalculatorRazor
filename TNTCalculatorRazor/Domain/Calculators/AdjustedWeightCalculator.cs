namespace TNTCalculatorRazor.Domain.Calculators;

public static class AdjustedWeightCalculator
{
    /// <summary>
    /// BMR 計算用の補正体重を算出
    /// 0歳児は必ず実測体重
    /// それ以外は肥満度により分岐
    /// </summary>
    public static double Calculate(
        int age,
        double actualWeight,
        double standardWeight,
        double obesityDegree )
    {
        // 0歳児は無条件で実測体重
        if (age == 0)
        {
            return actualWeight;
        }

        if (obesityDegree <= 80.0)
        {
            // 低体重：標準体重
            return standardWeight;
        }
        else if (obesityDegree >= 120.0)
        {
            // 肥満：調整体重
            return (actualWeight - standardWeight) * 0.25 + standardWeight;
        }
        else
        {
            // 通常：実測体重
            return actualWeight;
        }
    }
}