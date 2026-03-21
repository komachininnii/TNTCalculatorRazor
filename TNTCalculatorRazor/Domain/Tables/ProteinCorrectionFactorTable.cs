using TNTCalculatorRazor.Domain.Enums;
namespace TNTCalculatorRazor.Domain.Tables;
public static class ProteinCorrectionFactorTable
{
    public static double Get(ProteinCorrectionType type) =>
        type switch
        {
            ProteinCorrectionType.None               => 1.0,
            ProteinCorrectionType.CKD3bTo5           => 0.7,
            ProteinCorrectionType.LiverCirrhosisPoor => 0.5,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "未定義の蛋白補正係数です。")
        };
}
