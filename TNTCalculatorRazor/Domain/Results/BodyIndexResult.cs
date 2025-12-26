namespace TNTCalculatorRazor.Domain.Results;

public class BodyIndexResult
{
    public double Bmi { get; init; }
    public double StandardWeight { get; init; }
        
    // 肥満度。0歳児では null
    public double? ObesityDegree { get; init; }
}