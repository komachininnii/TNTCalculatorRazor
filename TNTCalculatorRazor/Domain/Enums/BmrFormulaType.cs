namespace TNTCalculatorRazor.Domain.Enums;

public enum BmrFormulaType
{
    Infant_KyotoPICU,           // 乳児（KPUM小児ICUマニュアル改定第7版）
    Child_Schofield1985,        // 小児（Schofield 1985）
    [Obsolete("小児BMRはSchofield(1985)へ移行。UIからは使用しない想定。")]
    Child_JapanDRI2025,         // （旧）小児（2025年 日本人の食事摂取基準）
    Adult_HarrisBenedict,       // 成人（Harris-Benedictの式）
    Adult_Ganpule2007           // 成人（2007年 Ganpuleらの式）
}
