namespace TNTCalculatorRazor.Domain.Constants;

// 入力値の制限を定義
public static class InputConstraints
{
    public const int AgeMin = 0;
    public const int AgeMax = 129;

    public const double HeightMin = 30.0;
    public const double HeightMax = 249.9;

    public const double WeightMin = 0.5;
    public const double WeightMax = 299.9;
}
