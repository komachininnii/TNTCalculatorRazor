namespace TNTCalculatorRazor.Domain.Models;

public sealed class EnteralFormulaDefinition
{
    public double KcalPerMl { get; init; }
    public double PackageKcal { get; init; }   // 1本あたり kcal

    // 成分比（1 mL あたり）
    public double ProteinPerMl { get; init; }
    public double FatPerMl { get; init; }
    public double CarboPerMl { get; init; }
    public double SaltPerMl { get; init; }
    public double VitKPerMl { get; init; }
    public double WaterPerMl { get; init; }
}
