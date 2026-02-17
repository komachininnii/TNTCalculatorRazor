namespace TNTCalculatorRazor.Domain.Enums;

public static class BmrFormulaTypeExtensions
{
    //public static string ToDisplayName( this BmrFormulaType formula )
    public static string ToShortName( this BmrFormulaType formula )
    {
        return formula switch
        {
            BmrFormulaType.Infant_KyotoPICU =>
                "Inf",

            BmrFormulaType.Child_Schofield1985 =>
                "Sch",

            BmrFormulaType.Child_JapanDRI2025 =>
                "DRI",

            BmrFormulaType.Adult_HarrisBenedict =>
                "HB",

            BmrFormulaType.Adult_Ganpule2007 =>
                "Gan",

            _ => "不明な式"
        };
    }
    public static string ToLongName( this BmrFormulaType formula )
    {
        return formula switch
        {
            BmrFormulaType.Infant_KyotoPICU => "乳児簡易式（KPUM小児ICUマニュアル 第7版）",
            BmrFormulaType.Child_Schofield1985 => "小児（Schofield 1985：体重ベース）",
            BmrFormulaType.Child_JapanDRI2025 => "小児（厚労省 基礎代謝基準値 2025）",
            BmrFormulaType.Adult_HarrisBenedict => "成人（Harris–Benedict）",
            BmrFormulaType.Adult_Ganpule2007 => "成人（Ganpule 2007）",
            _ => "不明な式"
        };
    }
}
