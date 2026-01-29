using TNTCalculatorRazor.Domain.Enums;

namespace TNTCalculatorRazor.Domain.Calculators;

public static class AdjustedWeightCalculator
{   
    // 調整体重の計算
    public static double CalculateAdjustedWeight( double actualWeight, double standardWeight )
         => (actualWeight - standardWeight) * 0.25 + standardWeight;
 
    public static BmrWeightBasisType GetBasis( int age, double obesityDegree )
    {
        if (age == 0) return BmrWeightBasisType.Actual;                 // 乳児は無条件で実測体重
        if (obesityDegree <= 80.0) return BmrWeightBasisType.Standard;  // 肥満度80%以下：標準体重
        if (obesityDegree >= 120.0) return BmrWeightBasisType.Adjusted; // 肥満度120%以上：調整体重
        return BmrWeightBasisType.Actual;                               // 通常：実測体重
    }

    // 肥満度等により、実測/標準/調整のいずれかを選択した「補正体重（最終採用体重）」を返す
    public static double CalculateCorrectedWeight(
        int age,
        double actualWeight,
        double standardWeight,
        double obesityDegree )
    {
        return GetBasis(age, obesityDegree) switch
        {
            BmrWeightBasisType.Standard => standardWeight,
            BmrWeightBasisType.Adjusted => CalculateAdjustedWeight(actualWeight, standardWeight), // 調整体重
            _ => actualWeight
        };
    }
}
