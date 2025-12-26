namespace TNTCalculatorRazor.Domain.Enums;

public static class BmrFormulaTypeExtensions
{
    public static string ToDisplayName( this BmrFormulaType formula )
    {
        return formula switch
        {
            BmrFormulaType.Infant_KyotoPICU =>
                "乳児：京都府立医大小児ICU式",

            BmrFormulaType.Child_JapanDRI2010 =>
                "小児：2010年 日本人の食事摂取基準",

            BmrFormulaType.Adult_HarrisBenedict =>
                "成人：Harris-Benedict式",

            BmrFormulaType.Adult_Ganpule2007 =>
                "成人：Ganpule ら（2007）",

            _ => "不明な式"
        };
    }
}