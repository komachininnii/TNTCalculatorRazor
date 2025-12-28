namespace TNTCalculatorRazor.Domain.Models;

public sealed class EnteralFeedingResult
{
    public double EnergyKcal { get; set; }
    public double VolumeMl { get; set; }

    public double ProteinG { get; set; }
    public double FatG { get; set; }
    public double CarbG { get; set; }
    public double SaltG { get; set; }
    public double VitaminKUg { get; set; }
    public double WaterMl { get; set; }
}
