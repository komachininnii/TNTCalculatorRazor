namespace TNTCalculatorRazor.Domain.Models;

/// <summary>
/// 経腸栄養 投与量・カロリー計算結果
/// </summary>
public sealed class EnteralDoseResult
{
    public double EnergyKcal { get; init; }
    public double VolumeMl { get; init; }

    // デバッグ・説明用
    public double VolumePerKcal { get; init; }
}
