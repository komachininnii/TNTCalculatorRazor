using TNTCalculatorRazor.Domain.Enums;

namespace TNTCalculatorRazor.Domain.Calculators;

public static class AdjustedWeightCalculator
{
    public static BmrWeightBasisType GetBasis( int age, double obesityDegree )
    {
        if (age == 0) return BmrWeightBasisType.Actual;                 // 乳児は無条件で実測体重
        if (obesityDegree <= 80.0) return BmrWeightBasisType.Standard;  // 肥満度80%以下：標準体重
        if (obesityDegree >= 120.0) return BmrWeightBasisType.Adjusted; // 肥満度120%以上：調整体重
        return BmrWeightBasisType.Actual;                               // 通常：実測体重
    }
    public static double Calculate(
        int age,
        double actualWeight,
        double standardWeight,
        double obesityDegree )
    {
        return GetBasis(age, obesityDegree) switch
        {
            BmrWeightBasisType.Standard => standardWeight,
            BmrWeightBasisType.Adjusted => (actualWeight - standardWeight) * 0.25 + standardWeight, // 調整体重
            _ => actualWeight
        };
    }
}
        /*
          
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
        */