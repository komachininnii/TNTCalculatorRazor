using TNTCalculatorRazor.Domain.Enums;

namespace TNTCalculatorRazor.Domain.Enums;

public static class BodyTemperatureLevelExtensions
{
    public static string ToDisplayName( this BodyTemperatureLevel level )
    {
        return level switch
        {
            BodyTemperatureLevel.Normal => "平熱（～36.9℃）",
            BodyTemperatureLevel.Fever37 => "37℃台",
            BodyTemperatureLevel.Fever38 => "38℃台",
            BodyTemperatureLevel.Fever39 => "39℃台",
            BodyTemperatureLevel.Fever40 => "40℃以上",
            _ => level.ToString()
        };
    }
}
