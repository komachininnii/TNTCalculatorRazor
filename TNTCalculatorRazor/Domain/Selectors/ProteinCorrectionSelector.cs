namespace TNTCalculatorRazor.Domain.Selectors;

using TNTCalculatorRazor.Domain.Enums;

public static class ProteinCorrectionSelector
{
    public static ProteinCorrectionType GetDefault(
        int age,
        ProteinCondition proteinCondition )
    {
        // 小児は自動補正なし
        if (age < 18)
            return ProteinCorrectionType.None;

        return proteinCondition switch
        {
            ProteinCondition.RenalFailure =>
                ProteinCorrectionType.CKD3bTo5,

            ProteinCondition.LiverCirrhosis =>
                ProteinCorrectionType.LiverCirrhosisPoor,

            _ => ProteinCorrectionType.None
        };
    }
}
