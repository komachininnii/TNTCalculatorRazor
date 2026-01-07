namespace TNTCalculatorRazor.Domain.Enums;

// 計算後の“種類ラベル”
public static class BmrWeightBasisTypeExtensions
{
    public static string ToKindName( this BmrWeightBasisType basis )
        => basis switch
        {
            BmrWeightBasisType.Actual => "実測",
            BmrWeightBasisType.Standard => "標準",
            BmrWeightBasisType.Adjusted => "調整",
            _ => basis.ToString()
        };

    public static string ToLongName( this BmrWeightBasisType basis )
        => basis switch
        {
            BmrWeightBasisType.Actual => "実測体重を使用",
            BmrWeightBasisType.Standard => "標準体重を使用",
            BmrWeightBasisType.Adjusted => "調整体重を使用",
            _ => basis.ToString()
        };
}

/*
public static class BmrWeightBasisTypeExtensions
{
    public static string ToDisplayName( this BmrWeightBasisType value )
    {
        var mem = value.GetType().GetMember(value.ToString()).FirstOrDefault();
        var attr = mem?.GetCustomAttribute<DisplayAttribute>();
        return attr?.Name ?? value.ToString();
    }
}
*/
