using System;

namespace TNTCalculatorRazor.Domain.Rules;

public static class RoundingRules
{
    // 必要エネルギー（kcal/day）を仕様として整数化する
    public static int RoundKcalToInt( double kcal )
        => (int)Math.Round(kcal, MidpointRounding.AwayFromZero);
    
    // 蛋白量などを小数 1 桁で確定させる
    public static double RoundGram1dp( double gram )
        => Math.Round(gram, 1, MidpointRounding.AwayFromZero);

    // 水分の mL 量を切り上げる
    public static int CeilMl( double ml )
        => (int)Math.Ceiling(ml);

    // 経腸栄養の mL を四捨五入で整数化
    public static int RoundEnteralMl( double ml )
        => (int)Math.Round(ml, MidpointRounding.AwayFromZero);
}
