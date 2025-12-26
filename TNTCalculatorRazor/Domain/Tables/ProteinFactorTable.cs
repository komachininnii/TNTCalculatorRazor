using TNTCalculatorRazor.Domain.Enums;

namespace TNTCalculatorRazor.Domain.Tables;

public static class ProteinFactorTable
{
    public static double Get( ProteinCondition condition )
    {
        return condition switch
        {
            ProteinCondition.RenalFailure => 0.8,
            ProteinCondition.Hemodialysis => 1.2,
            ProteinCondition.LiverCirrhosis => 1.0,
            _ => 1.0
        };
    }
}