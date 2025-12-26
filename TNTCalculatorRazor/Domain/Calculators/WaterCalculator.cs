using TNTCalculatorRazor.Domain.Enums;

namespace TNTCalculatorRazor.Domain.Calculators;

public static class WaterCalculator
{
    //=========================
    // 基本水分量
    //=========================
    public static double CalculateBase(
        int age,
        double actualWeight,
        bool isHemodialysis,
        bool isPregnant,
        double adjustedWeight,
        double obesityDegree )
    {   
        // 血液透析：ドライウェイト体重。即ち入力された体重（実測体重）を用いる。15mL/kg
        if (isHemodialysis)
        {
            return 15.0 * actualWeight;
        }

        // 乳児：実測体重を用いる。150mL/kg
        if (age == 0)
        {
            return 150.0 * actualWeight;
        }

        // 小児：実測体重を用いる。Holliday-Segarの式
        if (age < 18)
        {
            if (actualWeight < 10)
                return 100.0 * actualWeight;
            else if (actualWeight < 20)
                return 1000.0 + (actualWeight - 10) * 50.0;
            else
                return 1500.0 + (actualWeight - 20) * 20.0;
        }

        // 成人：妊娠かつ肥満度120%以上では調整体重を用いる。
        double weightForCalc =
            isPregnant && obesityDegree >= 120
                ? adjustedWeight
                : actualWeight;

        if (age <= 55)
            return 35.0 * weightForCalc;
        else if (age <= 65)
            return 30.0 * weightForCalc;
        else
            return 25.0 * weightForCalc;
    }

    //=========================
    // 発熱補正
    //=========================
    public static double CalculateFeverCorrection(
        BodyTemperatureLevel level,
        double actualWeight )
    {
        int step = (int)level;

        if (step == 0)
            return 0.0;

        if (actualWeight < 15.0)
        {
            // 体重15kg未満では10mL/kg
            return step * 10.0 * actualWeight;
        }
        else
        {
            return step * 150.0;
        }
    }

    //=========================
    // 総水分量
    //=========================
    public static double CalculateTotal(
    int age,
    double actualWeight,
    bool isHemodialysis,
    bool isPregnant,
    double adjustedWeight,
    double obesityDegree,
    BodyTemperatureLevel temperatureLevel )
    {
        double baseWater =
            CalculateBase(
                age,
                actualWeight,
                isHemodialysis,
                isPregnant,
                adjustedWeight,
                obesityDegree);

        if (isHemodialysis)
        {
            //透析では発熱補正なし
            return baseWater;
        }

        return baseWater
             + CalculateFeverCorrection(temperatureLevel, actualWeight);
    }
}
