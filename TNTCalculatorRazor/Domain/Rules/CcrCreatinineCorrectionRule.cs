using TNTCalculatorRazor.Domain.Enums;

namespace TNTCalculatorRazor.Domain.Rules;

public static class CcrCreatinineCorrectionRule
{
    public static CcrCreatinineCorrectionType GetType( int age, GenderType gender, double creatinineMgDl )
    {
        if (age < 70) return CcrCreatinineCorrectionType.None;

        return gender switch
        {
            GenderType.Male when creatinineMgDl < 0.8 => CcrCreatinineCorrectionType.Male70Plus_Min08,
            GenderType.Female when creatinineMgDl < 0.6 => CcrCreatinineCorrectionType.Female70Plus_Min06,
            _ => CcrCreatinineCorrectionType.None
        };
    }

    public static double? GetCorrectedCreatinine( int age, GenderType gender, double creatinineMgDl )
    {
        return GetType(age, gender, creatinineMgDl) switch
        {
            CcrCreatinineCorrectionType.Male70Plus_Min08 => 0.8,
            CcrCreatinineCorrectionType.Female70Plus_Min06 => 0.6,
            _ => null
        };
    }

    public static string GetNote( CcrCreatinineCorrectionType type )
    {
        return type switch
        {
            CcrCreatinineCorrectionType.Male70Plus_Min08 => "※筋量低下なら補正(Cr0.8)",
            CcrCreatinineCorrectionType.Female70Plus_Min06 => "※筋量低下なら補正(Cr0.6)",
            _ => ""
        };
    }
}
