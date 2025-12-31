namespace TNTCalculatorRazor.Domain.Enums;

using System.ComponentModel.DataAnnotations;
using System.Reflection;

public static class BmrWeightBasisTypeExtensions
{
    public static string ToDisplayName( this BmrWeightBasisType value )
    {
        var mem = value.GetType().GetMember(value.ToString()).FirstOrDefault();
        var attr = mem?.GetCustomAttribute<DisplayAttribute>();
        return attr?.Name ?? value.ToString();
    }
}
