namespace TNTCalculatorRazor.Domain.Models;

/// <summary>
/// 経腸栄養剤の「kcal あたり比率」
/// </summary>
public sealed class EnteralFormulaComposition
{
    public double VolumePerKcal { get; }
    public double ProteinPerKcal { get; }
    public double FatPerKcal { get; }
    public double CarbPerKcal { get; }
    public double SaltPerKcal { get; }
    public double VitaminKPerKcal { get; }
    public double WaterPerKcal { get; }

    public EnteralFormulaComposition(
        double volumePerKcal,
        double proteinPerKcal,
        double fatPerKcal,
        double carbPerKcal,
        double saltPerKcal,
        double vitaminKPerKcal,
        double waterPerKcal )
    {
        VolumePerKcal = volumePerKcal;
        ProteinPerKcal = proteinPerKcal;
        FatPerKcal = fatPerKcal;
        CarbPerKcal = carbPerKcal;
        SaltPerKcal = saltPerKcal;
        VitaminKPerKcal = vitaminKPerKcal;
        WaterPerKcal = waterPerKcal;
    }
}