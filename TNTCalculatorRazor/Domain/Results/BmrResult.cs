using TNTCalculatorRazor.Domain.Enums;
using TNTCalculatorRazor.Domain.Rules;

namespace TNTCalculatorRazor.Domain.Results;

public class BmrResult
{
    /// <summary>
    /// 内部計算用（補正・係数計算に使用）
    /// </summary>
    public double RawValue { get; init; }

    /// <summary>
    /// 表示用（四捨五入）
    /// </summary>
    public int DisplayValue =>
        RoundingRules.RoundKcalToInt(RawValue);
    public BmrFormulaType Formula { get; init; }
}
