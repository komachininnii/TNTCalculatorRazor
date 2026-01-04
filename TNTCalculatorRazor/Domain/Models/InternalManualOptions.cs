namespace TNTCalculatorRazor.Domain.Models;

public sealed class InternalManualOptions
{
    public bool Enabled { get; init; } = false;
    public string? Url { get; init; }
}
