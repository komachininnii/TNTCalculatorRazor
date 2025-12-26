using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection;
using TNTCalculatorRazor.Domain.Calculators;
using TNTCalculatorRazor.Domain.Enums;
using TNTCalculatorRazor.Domain.Results;
using TNTCalculatorRazor.Domain.Selectors;
using TNTCalculatorRazor.Domain.Tables;

public class IndexModel : PageModel
{
    [BindProperty] public int Age { get; set; }
    [BindProperty] public double Height { get; set; }
    [BindProperty] public double Weight { get; set; }
    [BindProperty] public Sex Sex { get; set; }
    [BindProperty] 
    public ProteinCondition SelectedProteinCondition { get; set; }
    [BindProperty]
    public ProteinCorrectionType SelectedProteinCorrection { get; set; }
           
    [BindProperty]
    public ActivityFactorType ActivityFactor { get; set; }
    = ActivityFactorType.BedriddenComa;
    

    [BindProperty]
    public StressFactorType StressFactor { get; set; }
        = StressFactorType.Normal;

    [BindProperty]
    public bool IsHemodialysis { get; set; }

    [BindProperty]
    public bool IsPregnant { get; set; }

    [BindProperty]
    public BodyTemperatureLevel SelectedBodyTemperature { get; set; }

    public BmrResult? BmrResult { get; set; }

    public BodyIndexResult? BodyIndex { get; set; }

    public double AdjustedWeight { get; private set; }
    // 体表面積
    public double BodySurfaceArea { get; private set; }

    // 表示用（係数そのもの）
    public double ActivityFactorValue { get; set; }
    public double StressFactorValue { get; set; }

   // Energy
    public double EnergyRaw { get; private set; }
    public int EnergyDisplay { get; private set; }

    // Protein
    public double ProteinRaw { get; private set; }
    public double ProteinDisplay { get; private set; }
    public string? ProteinDisplayText { get; private set; }
    // Water
    public double WaterRaw { get; private set; }
    public int WaterDisplay { get; private set; }
    public bool WaterFeverCorrected { get; private set; }

    // 蛋白質疾患補正係数（1.0 / 0.7 / 0.5）
    public double ProteinCorrect { get; set; } = 1.0;
           
    //Stress係数内訳（表示用）
    public double StressBase { get; private set; }
    public double StressTemperature { get; private set; }
    public double StressTotal { get; private set; }


  
    public void OnPost()
    {
        //==============================
        // 0. 蛋白補正の自動初期化
        //==============================
        var autoCorrection =
            ProteinCorrectionSelector.GetDefault(
                Age,
                SelectedProteinCondition);

        // ユーザー未選択時のみ自動設定
        if (SelectedProteinCorrection == ProteinCorrectionType.None)
        {
            SelectedProteinCorrection = autoCorrection;
        }

        if (ProteinCorrect <= 0)
        {
            ProteinCorrect = 1.0;
        }

        //==============================
        // ① 表示用BMR（実測体重）
        //==============================
        BmrResult = BmrCalculator.Calculate(
            Age,
            Weight,
            Height,
            Sex);

        //==============================
        // ② 標準体重・肥満度
        //==============================
        BodyIndex = BodyIndexCalculator.Calculate(
            Age,
            Height,
            Weight,
            Sex);
        
        //==============================
        // ★ 体表面積（BSA）
        //==============================
        BodySurfaceArea =
            BodySurfaceAreaCalculator.Calculate(
                heightCm: Height,
                weightKg: Weight);
        
        //==============================
        // ③ 補正体重
        //==============================
        AdjustedWeight = AdjustedWeightCalculator.Calculate(
            age: Age,
            actualWeight: Weight,
            standardWeight: BodyIndex.StandardWeight,
            obesityDegree: BodyIndex.ObesityDegree ?? 0);

        //==============================
        // ④ 栄養計算用体重
        //==============================
        double weightForEnergy =
            WeightForCalculationSelector.Select(
                usage: WeightUsage.Energy,
                age: Age,
                actualWeight: Weight,
                adjustedWeight: AdjustedWeight,
                standardWeight: BodyIndex.StandardWeight,
                proteinCondition: ProteinCondition.None);

        double weightForProtein =
            WeightForCalculationSelector.Select(
                usage: WeightUsage.Protein,
                age: Age,
                actualWeight: Weight,
                adjustedWeight: AdjustedWeight,
                standardWeight: BodyIndex.StandardWeight,
                proteinCondition: SelectedProteinCondition);

        //==============================
        // ⑤ 栄養計算用BMR（非表示）
        //==============================
        var bmrForEnergy = BmrCalculator.Calculate(
            Age,
            weightForEnergy,
            Height,
            Sex);

        //==============================
        // ⑥ 活動係数・ストレス係数
        //==============================
        var activity =
            ActivityFactorTable.Get(ActivityFactor);

        double stressBase =
            StressFactorTable.Get(StressFactor);

        double stressTemperature =
            TemperatureStressTable.GetAddition(
                SelectedBodyTemperature);

        double stressTotal =
            stressBase + stressTemperature;

        StressBase = stressBase;
        StressTemperature = stressTemperature;
        StressTotal = stressTotal;

        //==============================
        // ⑦ 推定必要カロリー
        //==============================
        if (Age == 0)
        {
            // 乳児
            double infantBmr = BmrResult!.RawValue;

            EnergyRaw =
                ((infantBmr * stressTotal)
                 + (40.0 * Weight))
                * 1.1;
        }
        else
        {
            EnergyRaw =
                bmrForEnergy.RawValue
                * activity
                * stressTotal;
        }

        EnergyDisplay =
            (int)Math.Round(
                EnergyRaw,
                MidpointRounding.AwayFromZero);

        //==============================
        // ⑧ 推定必要蛋白質量
        //==============================
        ProteinRaw =
            ProteinCalculator.Calculate(
                age: Age,
                weightForProtein: weightForProtein,
                stressFactor: stressTotal,
                proteinCorrect: ProteinCorrect);

        ProteinDisplay =
            Math.Round(
                ProteinRaw,
                1,
                MidpointRounding.AwayFromZero);

        ProteinDisplayText =
            ProteinDisplay.ToString("F1");

        //==============================
        // ⑨ 水分量
        //==============================
        WaterRaw =
            WaterCalculator.CalculateTotal(
                age: Age,
                actualWeight: Weight,
                isHemodialysis: IsHemodialysis,
                isPregnant: IsPregnant,
                adjustedWeight: AdjustedWeight,
                obesityDegree: BodyIndex.ObesityDegree ?? 0,
                temperatureLevel: SelectedBodyTemperature);

        WaterDisplay =
            (int)Math.Round(
                WaterRaw,
                MidpointRounding.AwayFromZero);

        // ★ 発熱補正が入ったか
        WaterFeverCorrected =
            !IsHemodialysis &&
            SelectedBodyTemperature != BodyTemperatureLevel.Normal;
    }


    /*
    // 疾患変更に応じた自動蛋白補正（初期値）
    var autoCorrection =
        ProteinCorrectionSelector.GetDefault(
            Age,
            SelectedProteinCondition);

    // ★ ユーザーがまだ None の場合のみ自動設定
    if (SelectedProteinCorrection == ProteinCorrectionType.None)
    {
        SelectedProteinCorrection = autoCorrection;
    }
    if (ProteinCorrect <= 0)
    {
        ProteinCorrect = 1.0;
    }

    // ストレス係数

      var baseStress = StressFactorTable.Get(StressFactor);
    var tempStress =
        TemperatureStressTable.GetAddition(SelectedBodyTemperature);

    double totalStress = baseStress + tempStress;




    //==============================
    // ① 表示用BMR（実測体重）
    //==============================
    BmrResult = BmrCalculator.Calculate(
        Age,
        Weight,
        Height,
        Sex);

    //==============================
    // ② 標準体重・肥満度
    //==============================
    BodyIndex = BodyIndexCalculator.Calculate(
        Age,
        Height,
        Weight,
        Sex);

    //==============================
    // ③ 補正体重（共通）
    //==============================
    AdjustedWeight = AdjustedWeightCalculator.Calculate(
        age: Age,
        actualWeight: Weight,
        standardWeight: BodyIndex.StandardWeight,
        obesityDegree: BodyIndex.ObesityDegree ?? 0);

    //==============================
    // ④ 栄養計算用体重（エネルギー）
    //==============================
    double weightForEnergy =
        WeightForCalculationSelector.Select(
            usage: WeightUsage.Energy,
            age: Age,
            actualWeight: Weight,
            adjustedWeight: AdjustedWeight,
            standardWeight: BodyIndex.StandardWeight,
            proteinCondition: ProteinCondition.None); // Energyでは未使用だが明示的に None

    //==============================
    // ⑤ 栄養計算用体重（蛋白質）
    //==============================
    double weightForProtein =
        WeightForCalculationSelector.Select(
            usage: WeightUsage.Protein,
            age: Age,
            actualWeight: Weight,
            adjustedWeight: AdjustedWeight,
            standardWeight: BodyIndex.StandardWeight,
            proteinCondition: SelectedProteinCondition);


    //==============================
    // ⑥ 栄養計算用BMR（再計算）
    //    ※ 表示しない
    //==============================
    var bmrForEnergy = BmrCalculator.Calculate(
        Age,
        weightForEnergy,
        Height,
        Sex);



    //==============================
    // ⑦ 活動係数・ストレス係数
    //==============================
    var activity = ActivityFactorTable.Get(ActivityFactor);
    // 基礎ストレス係数（感染・外傷など）
    double baseStress =
        StressFactorTable.Get(StressFactor);

    // 体温による加算
    double temperatureStress =
        TemperatureStressTable.GetAddition(SelectedBodyTemperature);

    // 合成ストレス係数
    double totalStress = baseStress + temperatureStress;

    StressBase = baseStress;
    StressTemperature = temperatureStress;
    StressTotal = totalStress;

    //==============================
    // ⑧ 推定必要カロリー
    //==============================
    if (Age == 0)
    {
        // 乳児（0歳）専用式
        // 実測体重BMR（表示用と同じ）
        double infantBmr = BmrResult!.RawValue;

        EnergyRaw =
            ((infantBmr * totalStress)
             + (40.0 * Weight))
            * 1.1;
    }
    else
    {
        // 小児・成人
        EnergyRaw =
            bmrForEnergy.RawValue
            * activity
            * totalStress;
    }

    EnergyDisplay =
        EnergyRaw.HasValue
            ? (int)Math.Round(
                EnergyRaw.Value,
                MidpointRounding.AwayFromZero)
            : null;

    //==============================
    // ⑨ 推定必要蛋白質量
    //==============================
    ActivityFactorValue = ActivityFactorTable.Get(ActivityFactor);
    StressFactorValue = StressTotal;

    ProteinRaw =
        ProteinCalculator.Calculate(
            age: Age,
            weightForProtein: weightForProtein,
            stressFactor: totalStress,
            proteinCorrect: ProteinCorrect);

    ProteinDisplay =
        ProteinRaw.HasValue
            ? Math.Round(
                ProteinRaw.Value,
                1,
                MidpointRounding.AwayFromZero)
            : null;

    ProteinDisplayText =
        ProteinDisplay.HasValue
            ? ProteinDisplay.Value.ToString("F1")
            : null;


    //==============================
    // ⑥ 水分量計算
    //==============================
    // 水分量
    WaterRaw =
        WaterCalculator.CalculateTotal(
            age: Age,
            actualWeight: Weight,
            isHemodialysis: IsHemodialysis,
            isPregnant: IsPregnant,
            adjustedWeight: AdjustedWeight,
            obesityDegree: BodyIndex.ObesityDegree ?? 0,
            temperatureLevel: SelectedBodyTemperature);

    WaterDisplay =
        (int)Math.Round(
            WaterRaw.Value,
            MidpointRounding.AwayFromZero);
    */
}
