namespace TNTCalculatorRazor.Domain.Tables;

using TNTCalculatorRazor.Domain.Enums;

public static class ProteinCorrectionTable
{
    public static double Get( ProteinCorrectionType type )
    {
        return type switch
        {
            ProteinCorrectionType.None => 1.0,
            ProteinCorrectionType.CKD3bTo5 => 0.7,
            ProteinCorrectionType.LiverCirrhosisPoor => 0.5,
            _ => 1.0
        };
    }
}