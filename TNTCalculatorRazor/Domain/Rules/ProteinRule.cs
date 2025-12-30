namespace TNTCalculatorRazor.Domain.Rules;

using TNTCalculatorRazor.Domain.Enums;

public static class ProteinRule
{
    public static bool IsStressFactorIgnored( int age, DiseaseType disease )
    {
        // 既存方針：小児は別ロジック（実測体重・自動補正なし）なので、成人のみ対象
        if (age < 18) return false;

        return disease is DiseaseType.RenalFailure
                     or DiseaseType.Hemodialysis
                     or DiseaseType.LiverCirrhosis;
    }

    public static bool UseStandardWeightForProtein( int age, DiseaseType disease )
    {
        if (age < 18) return false;

        return disease is DiseaseType.RenalFailure
                     or DiseaseType.Hemodialysis
                     or DiseaseType.LiverCirrhosis;
    }
}