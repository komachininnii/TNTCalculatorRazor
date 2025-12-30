namespace TNTCalculatorRazor.Domain.Selectors;

using TNTCalculatorRazor.Domain.Enums;

public static class ProteinCorrectionSelector
{
    public static ProteinCorrectionType GetDefault(
        int age,
        DiseaseType disease)
    {
        // 小児は自動補正なし
        if (age < 18)
            return ProteinCorrectionType.None;

        return disease switch
        {
            DiseaseType.RenalFailure =>
                ProteinCorrectionType.CKD3bTo5,

            // 肝硬変のデフォルトは 1.0（蛋白不耐はUIチェックで0.5に倒す）
            DiseaseType.LiverCirrhosis =>
                ProteinCorrectionType.None,


            _ => ProteinCorrectionType.None
        };
    }
}
