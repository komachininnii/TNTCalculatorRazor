namespace TNTCalculatorRazor.Domain.Rules;

using TNTCalculatorRazor.Domain.Enums;

public static class ProteinRule
{
    public static bool IsStressFactorIgnored( int age, DiseaseType disease )
        => IsProteinExceptionDisease(age, disease);

    public static bool UseStandardWeightForProtein( int age, DiseaseType disease )
        => IsProteinExceptionDisease(age, disease);

    private static bool IsProteinExceptionDisease( int age, DiseaseType disease )
    {
        // 小児は別ロジック（疾患指定なし）なので、成人のみ対象
        if (age < 18) return false;

        return disease is DiseaseType.RenalFailure
                     or DiseaseType.Hemodialysis
                     or DiseaseType.LiverCirrhosis;
    }
}
