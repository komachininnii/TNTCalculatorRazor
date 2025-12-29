using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TNTCalculatorRazor.Domain.Calculators;
using TNTCalculatorRazor.Domain.Enums;
using TNTCalculatorRazor.Domain.Results;
using TNTCalculatorRazor.Domain.Selectors;
using TNTCalculatorRazor.Domain.Services;
using TNTCalculatorRazor.Domain.Tables;

public class IndexModel : PageModel
{
    //==============================
    // 入力（Bind）
    //==============================
    // 基本入力（空欄スタート）
    [BindProperty] public int? Age { get; set; }                 // 年齢（年） null=未入力, 0=乳児
    [BindProperty] public double? Height { get; set; }           // 身長（cm） null=未入力
    [BindProperty] public double? Weight { get; set; }           // 体重（kg） null=未入力
    [BindProperty] public GenderType Gender { get; set; } = GenderType.Male;

    [BindProperty] public ActivityFactorType ActivityFactor { get; set; } = ActivityFactorType.BedriddenComa;
    [BindProperty] public StressFactorType StressFactor { get; set; } = StressFactorType.Normal;
    [BindProperty] public BodyTemperatureLevel SelectedBodyTemperature { get; set; } = BodyTemperatureLevel.Normal;

    [BindProperty] public DiseaseType SelectedDisease { get; set; } = DiseaseType.None;
    [BindProperty] public ProteinCorrectionType SelectedProteinCorrection { get; set; } = ProteinCorrectionType.None;

    // エネルギー算出方法（疾患でデフォルト切替）
    [BindProperty] public EnergyOrderType SelectedEnergyOrder { get; set; } = EnergyOrderType.BmrEstimated;
    [BindProperty] public int? ManualEnergyValue { get; set; }

    // 経腸栄養側の編集可能な「投与カロリー」
    [BindProperty] public int? EnergyOrderValue { get; set; }

    // ユーザーが投与カロリー/投与量を手で触ったか（疾患デフォルト上書き防止）
    [BindProperty] public bool IsEnergyUserEdited { get; set; } = false;

    [BindProperty] public bool IsHemodialysis { get; set; }
    [BindProperty] public bool IsPregnant { get; set; }

    // 経腸栄養
    [BindProperty] public EnteralFormulaType? SelectedEnteralFormula { get; set; }
    [BindProperty] public int? EnteralVolumeInput { get; set; }  // mL/day 手入力（端数調整用）
    [BindProperty] public string? Action { get; set; }           // hidden
    [BindProperty] public bool HasUserSelectedPackage { get; set; }

    //==============================
    // 計算結果（表示用）
    //==============================
    public BmrResult? BmrResult { get; private set; }
    public BodyIndexResult? BodyIndex { get; private set; }

    public double? AdjustedWeight { get; private set; }
    public double? BodySurfaceArea { get; private set; }

    // ストレス内訳
    public double StressBase { get; private set; }
    public double StressTemperature { get; private set; }
    public double StressTotal { get; private set; }

    // エネルギー候補（表示用）
    public int? BmrKcal { get; private set; }
    public int? Kcal25 { get; private set; }
    public int? Kcal30 { get; private set; }
    public int? Kcal35 { get; private set; }

    // 計算した最終値（参考表示用）
    public int? EnergyFinal { get; private set; }                // SelectedEnergyOrder + Manual の結果
    public double? ProteinRaw { get; private set; }
    public string ProteinDisplayText { get; private set; } = "";
    public int? WaterDisplay { get; private set; }
    public bool WaterFeverCorrected { get; private set; }

    //==============================
    // 経腸栄養（表示）
    //==============================
    public double? EnteralEnergy { get; private set; }           // kcal/day（表示投与量ベース）
    public double? EnteralVolume { get; private set; }           // mL/day（表示投与量）

    public double? EnteralProtein { get; private set; }
    public double? EnteralFat { get; private set; }
    public double? EnteralCarb { get; private set; }
    public double? EnteralSalt { get; private set; }
    public double? EnteralVitaminK { get; private set; }
    public double? EnteralWater { get; private set; }

    public IReadOnlyList<EnteralPackagePlan> EnteralPackagePlans { get; private set; }
        = Array.Empty<EnteralPackagePlan>();

    public List<SelectListItem> PackageVolumeOptions { get; private set; } = new();

    public static string FormatPlan( EnteralPackagePlan plan )
    {
        var parts = plan.CountsByVolume
            .OrderByDescending(kv => kv.Key)
            .Select(kv => $"{kv.Key}mL×{kv.Value}");

        var body = string.Join(" + ", parts);
        return plan.RemainderMl > 0 ? $"{body} + {plan.RemainderMl}mL" : body;
    }

    //==============================
    // POST
    //==============================
    public void OnPost()
    {
        var act = (Action ?? "").ToLowerInvariant();

        // 「ユーザーが編集した」判定（投与カロリー or 投与量）
        if (act == "energy" || act == "volume")
            IsEnergyUserEdited = true;

        // 1) 基本計算（入力が揃っていて範囲内なら）
        RecalcBase();

        // 2) 疾患によるデフォルト算出法の切替（ただしユーザー編集後は尊重）
        if (act == "disease")
        {
            SelectedEnergyOrder = EnergyOrderDefaultSelector.GetDefault(SelectedDisease);
        }

        // 3) EnergyOrderValue（経腸栄養側の投与カロリー）を候補から自動入力
        //    - 疾患変更時 / 算出法変更時：ユーザーがまだ上書きしていない場合のみ
        if ((act == "disease" || act == "order") && !IsEnergyUserEdited)
        {
            EnergyOrderValue = SelectedEnergyOrder switch
            {
                EnergyOrderType.BmrEstimated => BmrKcal,
                EnergyOrderType.Kcal25 => Kcal25,
                EnergyOrderType.Kcal30 => Kcal30,
                EnergyOrderType.Kcal35 => Kcal35,
                _ => EnergyOrderValue
            };

            ModelState.Remove(nameof(EnergyOrderValue));
        }

        // 4) 蛋白補正のデフォルト（年齢が入っているときだけ）
        if (SelectedProteinCorrection == ProteinCorrectionType.None && Age.HasValue)
        {
            SelectedProteinCorrection =
                ProteinCorrectionSelector.GetDefault(Age.Value, SelectedDisease);
        }

        // 5) 参考として EnergyFinal / Protein / Water も更新（入力が揃っているときだけ）
        RecalcEnergyProteinWater();

        // 6) 経腸栄養（kcal?mL連動、必要量ベース + 割付候補）
        RecalcEnteral();
    }

    //==============================
    // 基本計算
    //==============================
    private bool CanCalcBase()
    {
        // 必須：年齢・身長・体重
        if (!Age.HasValue || !Height.HasValue || !Weight.HasValue)
            return false;

        // 範囲（「あり得ない値」を弾く）
        if (Age.Value < 0 || Age.Value >= 130) return false;
        if (Height.Value < 30 || Height.Value >= 250) return false;
        if (Weight.Value < 0.5 || Weight.Value >= 300) return false;

        return true;
    }

    private void RecalcBase()
    {
        // 初期化
        BmrResult = null;
        BodyIndex = null;
        BodySurfaceArea = null;
        AdjustedWeight = null;

        BmrKcal = Kcal25 = Kcal30 = Kcal35 = null;

        if (!CanCalcBase())
            return;

        // BMR / 体格 / BSA
        BmrResult = BmrCalculator.Calculate(Age!.Value, Weight!.Value, Height!.Value, Gender);
        BodyIndex = BodyIndexCalculator.Calculate(Age.Value, Height.Value, Weight.Value, Gender);
        BodySurfaceArea = BodySurfaceAreaCalculator.Calculate(Height.Value, Weight.Value);

        AdjustedWeight = AdjustedWeightCalculator.Calculate(
            Age.Value,
            Weight.Value,
            BodyIndex.StandardWeight,
            BodyIndex.ObesityDegree ?? 0);

        // 表示用エネルギー候補（整数）
        BmrKcal = (int)Math.Round(BmrResult.RawValue, MidpointRounding.AwayFromZero);

        // 25/30/35 は標準体重ベース（年齢に関係なく表示する方針に寄せる）
        // ※ StandardWeight が計算できている前提
        Kcal25 = (int)Math.Round(BodyIndex.StandardWeight * 25.0, MidpointRounding.AwayFromZero);
        Kcal30 = (int)Math.Round(BodyIndex.StandardWeight * 30.0, MidpointRounding.AwayFromZero);
        Kcal35 = (int)Math.Round(BodyIndex.StandardWeight * 35.0, MidpointRounding.AwayFromZero);
    }

    //==============================
    // エネルギー/蛋白/水分（参考表示）
    //==============================
    private void RecalcEnergyProteinWater()
    {
        EnergyFinal = null;
        ProteinRaw = null;
        ProteinDisplayText = "";
        WaterDisplay = null;
        WaterFeverCorrected = false;

        // ストレスは入力が揃わなくても計算可能
        StressBase = StressFactorTable.Get(StressFactor);
        StressTemperature = TemperatureStressTable.GetAddition(SelectedBodyTemperature);
        StressTotal = StressBase + StressTemperature;

        if (!CanCalcBase() || BodyIndex is null || AdjustedWeight is null || BmrResult is null)
            return;

        // BMR推定エネルギー
        var bmrForEnergy = BmrCalculator.Calculate(Age!.Value, AdjustedWeight.Value, Height!.Value, Gender);

        var energyByBmr =
            Age.Value == 0
                ? ((BmrResult.RawValue * StressTotal) + (40 * Weight!.Value)) * 1.1
                : bmrForEnergy.RawValue
                    * ActivityFactorTable.Get(ActivityFactor)
                    * StressTotal;

        // kcal/kg（標準体重）
        var e25 = 25 * BodyIndex.StandardWeight;
        var e30 = 30 * BodyIndex.StandardWeight;
        var e35 = 35 * BodyIndex.StandardWeight;

        double selected =
            SelectedEnergyOrder switch
            {
                EnergyOrderType.Kcal25 => e25,
                EnergyOrderType.Kcal30 => e30,
                EnergyOrderType.Kcal35 => e35,
                EnergyOrderType.Manual => ManualEnergyValue ?? energyByBmr,
                _ => energyByBmr
            };

        EnergyFinal = (int)Math.Round(selected, MidpointRounding.AwayFromZero);

        // 蛋白補正係数
        double proteinCorrect =
            SelectedProteinCorrection switch
            {
                ProteinCorrectionType.CKD3bTo5 => 0.7,
                ProteinCorrectionType.LiverCirrhosisPoor => 0.5,
                _ => 1.0
            };

        // 蛋白（既存ロジック踏襲）
        double weightForProtein =
            WeightForCalculationSelector.Select(
                WeightUsage.Protein,
                Age.Value,
                Weight.Value,
                AdjustedWeight.Value,
                BodyIndex.StandardWeight,
                SelectedDisease);

        ProteinRaw =
            ProteinCalculator.Calculate(
                Age.Value,
                weightForProtein,
                StressTotal,
                proteinCorrect);

        ProteinDisplayText =
            Math.Round(ProteinRaw.Value, 1, MidpointRounding.AwayFromZero).ToString("F1");

        // 水分（既存ロジック踏襲）
        double water =
            WaterCalculator.CalculateTotal(
                Age.Value,
                Weight.Value,
                IsHemodialysis,
                IsPregnant,
                AdjustedWeight.Value,
                BodyIndex.ObesityDegree ?? 0,
                SelectedBodyTemperature);

        WaterDisplay = (int)Math.Round(water, MidpointRounding.AwayFromZero);

        WaterFeverCorrected =
            !IsHemodialysis &&
            SelectedBodyTemperature != BodyTemperatureLevel.Normal;
    }

    //==============================
    // 経腸栄養（必要量ベース + 割付候補）
    //==============================
    private void RecalcEnteral()
    {
        // 初期化
        EnteralPackagePlans = Array.Empty<EnteralPackagePlan>();
        PackageVolumeOptions = new List<SelectListItem>();

        EnteralEnergy = null;
        EnteralVolume = null;

        EnteralProtein = null;
        EnteralFat = null;
        EnteralCarb = null;
        EnteralSalt = null;
        EnteralVitaminK = null;
        EnteralWater = null;

        var act = (Action ?? "").ToLowerInvariant();

        if (!SelectedEnteralFormula.HasValue)
        {
            EnteralVolumeInput = null;
            ModelState.Remove(nameof(EnteralVolumeInput));
            return;
        }

        var formula = SelectedEnteralFormula.Value;
        var comp = EnteralFormulaTable.Get(formula);
        var packageVolumes = EnteralPackageTable.Get(formula);

        PackageVolumeOptions = packageVolumes
            .Select(v => new SelectListItem($"{v} mL", v.ToString()))
            .ToList();

        var maxToShow = packageVolumes.Count <= 1 ? 1 : 2;

        // volume のときだけ mL を優先。その他は kcal→mL を再計算して入力欄も同期。
        if (act == "volume" && EnteralVolumeInput.HasValue && EnteralVolumeInput.Value > 0)
        {
            EnteralVolume = EnteralVolumeInput.Value;

            // mL → kcal
            var kcal = EnteralEnergyCalculator.CalculateEnergyFromVolume(EnteralVolume.Value, comp);
            var kcalRounded = (int)Math.Round(kcal, MidpointRounding.AwayFromZero);

            EnteralEnergy = kcalRounded;

            // kcal入力欄も同期
            EnergyOrderValue = kcalRounded;
            ModelState.Remove(nameof(EnergyOrderValue));

            // 割付候補（目安）
            EnteralPackagePlans =
                EnteralPackageAllocator.BuildPlans(
                    (int)Math.Round(EnteralVolume.Value, MidpointRounding.AwayFromZero),
                    packageVolumes.ToList(),
                    maxPlans: maxToShow);
        }
        else if (EnergyOrderValue.HasValue && EnergyOrderValue.Value > 0)
        {
            var targetKcal = EnergyOrderValue.Value;

            // kcal → 必要mL（端数含む）
            var rawVolume = targetKcal * comp.VolumePerKcal;
            var targetVolumeMl = (int)Math.Round(rawVolume, MidpointRounding.AwayFromZero);

            EnteralVolume = targetVolumeMl;

            // 入力欄も同期
            EnteralVolumeInput = targetVolumeMl;
            ModelState.Remove(nameof(EnteralVolumeInput));

            // 表示投与量ベースの kcal（厳密には targetKcal とほぼ一致する想定）
            EnteralEnergy = EnteralEnergyCalculator.CalculateEnergyFromVolume(EnteralVolume.Value, comp);

            // 割付候補（目安）
            EnteralPackagePlans =
                EnteralPackageAllocator.BuildPlans(
                    targetVolumeMl,
                    packageVolumes.ToList(),
                    maxPlans: maxToShow);
        }
        else
        {
            // kcal も mL も無い
            EnteralVolumeInput = null;
            ModelState.Remove(nameof(EnteralVolumeInput));
            return;
        }

        // 成分計算（常に「表示されている投与量」から）
        if (EnteralEnergy.HasValue)
        {
            EnteralProtein = EnteralEnergy.Value * comp.ProteinPerKcal;
            EnteralFat = EnteralEnergy.Value * comp.FatPerKcal;
            EnteralCarb = EnteralEnergy.Value * comp.CarbPerKcal;
            EnteralSalt = EnteralEnergy.Value * comp.SaltPerKcal;
            EnteralVitaminK = EnteralEnergy.Value * comp.VitaminKPerKcal;
            EnteralWater = EnteralEnergy.Value * comp.WaterPerKcal;
        }
    }
}



/*
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TNTCalculatorRazor.Domain.Calculators;
using TNTCalculatorRazor.Domain.Enums;
using TNTCalculatorRazor.Domain.Models;
using TNTCalculatorRazor.Domain.Results;
using TNTCalculatorRazor.Domain.Selectors;
using TNTCalculatorRazor.Domain.Tables;
using TNTCalculatorRazor.Domain.Services;

public class IndexModel : PageModel
{
    //==============================
    // 入力（Bind）
    //==============================
    // ===== 体格入力（UI） =====
    [BindProperty] public int? Age { get; set; }            // 年齢（年）
    [BindProperty] public int? Height { get; set; }         // 身長（cm）
    [BindProperty] public double? Weight { get; set; }      // 体重（kg）
    [BindProperty] public GenderType Gender { get; set; } = GenderType.Male; // 性別

    // ===== 表示用（自動計算） =====
    public int? BmrKcal { get; private set; }
    public int? Kcal25 { get; private set; }
    public int? Kcal30 { get; private set; }
    public int? Kcal35 { get; private set; }

    // ===== ユーザーが投与カロリーを手でいじったか =====
    [BindProperty] public bool IsEnergyUserEdited { get; set; } = false;
   
    [BindProperty] public ActivityFactorType ActivityFactor { get; set; } = ActivityFactorType.BedriddenComa;
    [BindProperty] public StressFactorType StressFactor { get; set; } = StressFactorType.Normal;
    [BindProperty] public BodyTemperatureLevel SelectedBodyTemperature { get; set; } = BodyTemperatureLevel.Normal;

    [BindProperty] public DiseaseType SelectedDisease { get; set; } = DiseaseType.None;

    [BindProperty] public ProteinCorrectionType SelectedProteinCorrection { get; set; } = ProteinCorrectionType.None;

    [BindProperty] public EnergyOrderType SelectedEnergyOrder { get; set; } = EnergyOrderType.BmrEstimated;
    [BindProperty] public int? ManualEnergyValue { get; set; }

    [BindProperty] public bool IsHemodialysis { get; set; }
    [BindProperty] public bool IsPregnant { get; set; }

    //==============================
    // 計算結果（表示用）
    //==============================
    public BmrResult? BmrResult { get; private set; }
    public BodyIndexResult? BodyIndex { get; private set; }

    public double AdjustedWeight { get; private set; }
    public double BodySurfaceArea { get; private set; }

    // エネルギー
    public double EnergyByBmr { get; private set; }
    public double Energy25 { get; private set; }
    public double Energy30 { get; private set; }
    public double Energy35 { get; private set; }
    public int EnergyFinal { get; private set; }

    // 蛋白
    public double ProteinRaw { get; private set; }
    public string ProteinDisplayText { get; private set; } = "";

    // 水分
    public int WaterDisplay { get; private set; }
    public bool WaterFeverCorrected { get; private set; }

    // ストレス内訳
    public double StressBase { get; private set; }
    public double StressTemperature { get; private set; }
    public double StressTotal { get; private set; }

    // 経腸栄養
    [BindProperty]
    public EnteralFormulaType? SelectedEnteralFormula { get; set; }

    [BindProperty]
    public double TargetEnergyKcal { get; set; }   // 投与カロリー（編集可）

    [BindProperty]
    public double EnteralVolumeMl { get; set; }    // 投与量（編集可）

    public EnteralFeedingResult? EnteralResult { get; private set; }

    // 規格量（mL）
    [BindProperty]
    public int SelectedPackageVolume { get; set; }

    // Select 用
    public List<SelectListItem> PackageVolumeOptions { get; private set; }
        = new();

    [BindProperty]
    public DiseaseType Disease { get; set; }


    // =========================
    // カロリー選択
    // =========================
    [BindProperty]
    public int? EnergyOrderValue { get; set; }

    [BindProperty]
    public bool IsEnergyManuallyEdited { get; set; }

    // =========================
    // 経腸栄養
    // =========================
    //[BindProperty]
    //public EnteralPackageSize SelectedPackage { get; set; }
    public double EnergyRaw { get; private set; }

    //==============================
    // 経腸栄養（Phase A）
    //==============================
    [BindProperty]
    public EnteralFormulaType? SelectedFormula { get; set; }

    // 投与カロリー（EnergyOrder から来る）
    public double? EnteralEnergy { get; private set; }

    // 自動算出される投与量（mL）
    //public double? EnteralVolume { get; private set; }

    // 成分表示
    public double? EnteralProtein { get; private set; }
    public double? EnteralFat { get; private set; }
    public double? EnteralCarb { get; private set; }
    public double? EnteralSalt { get; private set; }
    public double? EnteralVitaminK { get; private set; }
    public double? EnteralWater { get; private set; }

    // Enteral

    [BindProperty]
    public double? EnteralVolume { get; set; }

    [BindProperty] public string? Action { get; set; }

    [BindProperty] public bool HasUserSelectedPackage { get; set; }

    public IReadOnlyList<EnteralPackagePlan> EnteralPackagePlans { get; private set; }
    = Array.Empty<EnteralPackagePlan>();

    public static string FormatPlan( EnteralPackagePlan plan )
    {
        // 例: 400mL×4 + 300mL×2 + 22mL
        var parts = plan.CountsByVolume
            .OrderByDescending(kv => kv.Key)
            .Select(kv => $"{kv.Key}mL×{kv.Value}");

        var body = string.Join(" + ", parts);

        return plan.RemainderMl > 0
            ? $"{body} + {plan.RemainderMl}mL"
            : body;
    }

    [BindProperty]
    public int? EnteralVolumeInput { get; set; }  // mL/day 手入力（端数調整用）



    public void OnPost( string action )
    {
        //--------------------------
        // 0. 自動蛋白補正
        //--------------------------
        if (SelectedProteinCorrection == ProteinCorrectionType.None)
        {
            SelectedProteinCorrection =
                ProteinCorrectionSelector.GetDefault(Age, SelectedDisease);
        }

        double proteinCorrect =
            SelectedProteinCorrection switch
            {
                ProteinCorrectionType.CKD3bTo5 => 0.7,
                ProteinCorrectionType.LiverCirrhosisPoor => 0.5,
                _ => 1.0
            };

        //--------------------------
        // 1. BMR（表示用）
        //--------------------------
        BmrResult = BmrCalculator.Calculate(Age, Weight, Height, Gender);

        //--------------------------
        // 2. 体格
        //--------------------------
        BodyIndex = BodyIndexCalculator.Calculate(Age, Height, Weight, Gender);

        BodySurfaceArea =
            BodySurfaceAreaCalculator.Calculate(Height, Weight);

        AdjustedWeight =
            AdjustedWeightCalculator.Calculate(
                Age,
                Weight,
                BodyIndex.StandardWeight,
                BodyIndex.ObesityDegree ?? 0);

        //--------------------------
        // 3. ストレス係数
        //--------------------------
        StressBase = StressFactorTable.Get(StressFactor);
        StressTemperature = TemperatureStressTable.GetAddition(SelectedBodyTemperature);
        StressTotal = StressBase + StressTemperature;

        //--------------------------
        // 4. BMR 推定エネルギー
        //--------------------------
        var bmrForEnergy =
            BmrCalculator.Calculate(Age, AdjustedWeight, Height, Gender);

        EnergyByBmr =
            Age == 0
                ? ((BmrResult!.RawValue * StressTotal) + (40 * Weight)) * 1.1
                : bmrForEnergy.RawValue
                    * ActivityFactorTable.Get(ActivityFactor)
                    * StressTotal;

        //--------------------------
        // 5. kcal/kg（成人）
        //--------------------------
        if (Age >= 18)
        {
            Energy25 = 25 * BodyIndex.StandardWeight;
            Energy30 = 30 * BodyIndex.StandardWeight;
            Energy35 = 35 * BodyIndex.StandardWeight;
        }

        //--------------------------
        // 6. 最終エネルギー
        //--------------------------
        double selected =
            SelectedEnergyOrder switch
            {
                EnergyOrderType.Kcal25 => Energy25,
                EnergyOrderType.Kcal30 => Energy30,
                EnergyOrderType.Kcal35 => Energy35,
                EnergyOrderType.Manual => ManualEnergyValue ?? EnergyByBmr,
                _ => EnergyByBmr
            };

        EnergyFinal =
            (int)Math.Round(selected, MidpointRounding.AwayFromZero);

        //--------------------------
        // 7. 蛋白
        //--------------------------
        double weightForProtein =
            WeightForCalculationSelector.Select(
                WeightUsage.Protein,
                Age,
                Weight,
                AdjustedWeight,
                BodyIndex.StandardWeight,
                SelectedDisease);

        ProteinRaw =
            ProteinCalculator.Calculate(
                Age,
                weightForProtein,
                StressTotal,
                proteinCorrect);

        ProteinDisplayText =
            Math.Round(ProteinRaw, 1, MidpointRounding.AwayFromZero)
                .ToString("F1");

        //--------------------------
        // 8. 水分
        //--------------------------
        double water =
            WaterCalculator.CalculateTotal(
                Age,
                Weight,
                IsHemodialysis,
                IsPregnant,
                AdjustedWeight,
                BodyIndex.ObesityDegree ?? 0,
                SelectedBodyTemperature);

        WaterDisplay =
            (int)Math.Round(water, MidpointRounding.AwayFromZero);

        WaterFeverCorrected =
            !IsHemodialysis &&
            SelectedBodyTemperature != BodyTemperatureLevel.Normal;

        // ==============================
        // 経腸栄養（必要量ベース + 割付候補）
        // ==============================
        // まず表示系を初期化（前回値が残らないように）
        EnteralPackagePlans = Array.Empty<EnteralPackagePlan>();
        PackageVolumeOptions = new List<SelectListItem>();

        // Action は hidden で Bind されている前提
        var act = (Action ?? "").ToLowerInvariant();

        if (SelectedEnteralFormula.HasValue)
        {
            var formula = SelectedEnteralFormula.Value;

            // テーブル取得
            var comp = EnteralFormulaTable.Get(formula);
            var packageVolumes = EnteralPackageTable.Get(formula);

            // 規格候補（将来 select に使うなら）
            PackageVolumeOptions = packageVolumes
                .Select(v => new SelectListItem($"{v} mL", v.ToString()))
                .ToList();

            // 割付候補は 1規格→1件、2規格以上→2件
            var maxToShow = packageVolumes.Count <= 1 ? 1 : 2;

            // 1) どちらの入力を優先するか決める
            //    volume のときだけ mL を優先。それ以外は kcal→mL を必ず再計算して入力欄も同期。
            if (act == "volume" && EnteralVolumeInput.HasValue && EnteralVolumeInput.Value > 0)
            {
                // --- mL手入力ルート ---
                EnteralVolume = EnteralVolumeInput.Value;

                // 表示投与量から kcal を逆算（成分の基準）
                EnteralEnergy =
                    EnteralEnergyCalculator.CalculateEnergyFromVolume(
                        EnteralVolume.Value,
                        comp);

                // ユーザーが mL を変えた結果なので、kcal入力側も同期しておく
                var kcalRounded = (int)Math.Round(EnteralEnergy.Value, MidpointRounding.AwayFromZero); // 整数化
                EnergyOrderValue = kcalRounded;
                // EnteralEnergy も表示と一致させたいのでこちらも揃える
                EnteralEnergy = kcalRounded;

                ModelState.Remove(nameof(EnergyOrderValue));

                // 割付候補（目安）
                EnteralPackagePlans =
                    EnteralPackageAllocator.BuildPlans(
                        (int)Math.Round(EnteralVolume.Value, MidpointRounding.AwayFromZero),
                        packageVolumes.ToList(),
                        maxPlans: maxToShow);
            }
            else if (EnergyOrderValue.HasValue && EnergyOrderValue.Value > 0)
            {
                // --- kcal入力ルート（製剤変更・カロリー変更でもここに入る） ---
                var targetKcal = EnergyOrderValue.Value;

                // 必要投与量（端数含む）
                var rawVolume = targetKcal * comp.VolumePerKcal;
                var targetVolumeMl = (int)Math.Round(rawVolume, MidpointRounding.AwayFromZero);

                EnteralVolume = targetVolumeMl;

                // ★ここが重要：投与量入力欄も常に同期させる
                EnteralVolumeInput = targetVolumeMl;

                // Razor Pages は ModelState が優先されるため、入力欄が古い値のままになることがある
                // → 同期したときは明示的に ModelState から消す
                ModelState.Remove(nameof(EnteralVolumeInput));

                // 成分の基準となる kcal（表示投与量ベース）
                EnteralEnergy =
                    EnteralEnergyCalculator.CalculateEnergyFromVolume(
                        EnteralVolume.Value,
                        comp);

                // 割付候補（目安）
                EnteralPackagePlans =
                    EnteralPackageAllocator.BuildPlans(
                        targetVolumeMl,
                        packageVolumes.ToList(),
                        maxPlans: maxToShow);
            }
            else
            {
                // kcal も mL も無い
                EnteralVolume = null;
                EnteralEnergy = null;
                EnteralVolumeInput = null;
                ModelState.Remove(nameof(EnteralVolumeInput));
            }

            // 2) 成分計算：常に「表示されている投与量」から（= EnteralEnergy から）
            if (EnteralEnergy.HasValue)
            {
                EnteralProtein = EnteralEnergy.Value * comp.ProteinPerKcal;
                EnteralFat = EnteralEnergy.Value * comp.FatPerKcal;
                EnteralCarb = EnteralEnergy.Value * comp.CarbPerKcal;
                EnteralSalt = EnteralEnergy.Value * comp.SaltPerKcal;
                EnteralVitaminK = EnteralEnergy.Value * comp.VitaminKPerKcal;
                EnteralWater = EnteralEnergy.Value * comp.WaterPerKcal;
            }
            else
            {
                EnteralProtein = null;
                EnteralFat = null;
                EnteralCarb = null;
                EnteralSalt = null;
                EnteralVitaminK = null;
                EnteralWater = null;
            }
        }
        else
        {
            // 製剤が未選択
            EnteralVolume = null;
            EnteralEnergy = null;
            EnteralVolumeInput = null;
            ModelState.Remove(nameof(EnteralVolumeInput));

            EnteralProtein = null;
            EnteralFat = null;
            EnteralCarb = null;
            EnteralSalt = null;
            EnteralVitaminK = null;
            EnteralWater = null;

            EnteralPackagePlans = Array.Empty<EnteralPackagePlan>();
            PackageVolumeOptions = new List<SelectListItem>();
        }
    }
}
        /*
        EnteralPackagePlans = Array.Empty<EnteralPackagePlan>();

        if (SelectedEnteralFormula.HasValue && EnergyOrderValue.HasValue)
        {
            var formula = SelectedEnteralFormula.Value;
            var comp = EnteralFormulaTable.Get(formula);

            // 規格量候補（1本あたりmL）
            var packageVolumes = EnteralPackageTable.Get(formula);

            // UIのselect候補（今後使うなら）
            PackageVolumeOptions = packageVolumes
                .Select(v => new SelectListItem($"{v} mL", v.ToString()))
                .ToList();

            // 1) kcal → 必要投与量（mL/day）
            //    ※ここは「表示されている投与量」を作る重要箇所
            var targetKcal = EnergyOrderValue.Value;

            // 理論必要量（mL）…端数が出る可能性あり
            var rawVolume = targetKcal * comp.VolumePerKcal; // double

            // UIでは mL を整数表示しているので、投与量は int に正規化
            // 端数の扱いはここで固定（まずは四捨五入：AwayFromZero）
            var targetVolumeMl = (int)Math.Round(rawVolume, MidpointRounding.AwayFromZero);

            EnteralVolume = targetVolumeMl;

            // 2) 割付候補を生成（例：400×4 + 300×2 + 22mL）
            //    ※「超えない範囲で余り最小」の候補が返る
            EnteralPackagePlans =
                EnteralPackageAllocator.BuildPlans(
                    targetVolumeMl,
                    packageVolumes.ToList(),
                    maxPlans: 5);
            // 最大2案まで表示
            var maxToShow = packageVolumes.Count <= 1 ? 1 : 2;
            EnteralPackagePlans = EnteralPackagePlans.Take(maxToShow).ToList();

            // 3) 成分計算は「表示されている投与量」から
            //    （= パッケージ丸めでは変えない）
            EnteralEnergy =
                EnteralEnergyCalculator.CalculateEnergyFromVolume(
                    EnteralVolume.Value,
                    comp);

            if (EnteralEnergy.HasValue)
            {
                EnteralProtein = EnteralEnergy.Value * comp.ProteinPerKcal;
                EnteralFat = EnteralEnergy.Value * comp.FatPerKcal;
                EnteralCarb = EnteralEnergy.Value * comp.CarbPerKcal;
                EnteralSalt = EnteralEnergy.Value * comp.SaltPerKcal;
                EnteralVitaminK = EnteralEnergy.Value * comp.VitaminKPerKcal;
                EnteralWater = EnteralEnergy.Value * comp.WaterPerKcal;
            }

            // ★重要：ここでは EnergyOrderValue を上書きしない
            // ユーザーが入力した kcal を尊重し、入力値変更で再計算するため
        }
        else
        {
            EnteralVolume = null;
            EnteralEnergy = null;
            EnteralProtein = null;
            EnteralFat = null;
            EnteralCarb = null;
            EnteralSalt = null;
            EnteralVitaminK = null;
            EnteralWater = null;
        }
    }
}


    /*
        // ==============================
        // 経腸栄養（統合版：action対応）
        // ==============================
        if (SelectedEnteralFormula.HasValue && EnergyOrderValue.HasValue)
        {
            var formula = SelectedEnteralFormula.Value;
            var comp = EnteralFormulaTable.Get(formula);

            // 規格量候補（intのリスト）
            var packageVolumes = EnteralPackageTable.Get(formula); // IEnumerable<int> 前提

            // UIのselect候補を作る（毎回作ってOK：確認用UIなら簡単が正義）
            PackageVolumeOptions = packageVolumes
                .Select(v => new SelectListItem($"{v} mL", v.ToString()))
                .ToList();

            // action判定
            var act = (Action ?? "").ToLowerInvariant();

            if (act == "formula")
            {
                // 製剤変更：候補が変わるので「ユーザー選択なし」に戻す
                HasUserSelectedPackage = false;
                SelectedPackageVolume = 0;
            }
            else if (act == "package")
            {
                // 規格量をユーザーが触った
                HasUserSelectedPackage = true;
            }
            else if (act == "disease")
            {
                //UI確認用
                SelectedEnergyOrder = EnergyOrderDefaultSelector.GetDefault(SelectedDisease);
                ManualEnergyValue = null;
                IsEnergyManuallyEdited = false;
            }
            else if (act == "order")
            {
                // 算出法変更時：手入力フラグを適切にリセット
                IsEnergyManuallyEdited = (SelectedEnergyOrder == EnergyOrderType.Manual);

                // Manual を外したら手入力値を無効化したいなら
                if (SelectedEnergyOrder != EnergyOrderType.Manual)
                    ManualEnergyValue = null;
            }


            // kcal→rawVolume
            var rawVolume = EnergyOrderValue.Value * comp.VolumePerKcal;

            // 初回だけ切り上げ（または候補外なら保険で切り上げ）
            var isSelectedValid = packageVolumes.Contains(SelectedPackageVolume);

            if (!HasUserSelectedPackage || SelectedPackageVolume == 0 || !isSelectedValid)
            {
                SelectedPackageVolume = EnteralPackageRounder.RoundUp(rawVolume, packageVolumes);
            }

            // 確定投与量（mL）
            EnteralVolume = SelectedPackageVolume;

            // mL → kcal（確定投与量ベースで再計算）
            EnteralEnergy =
                EnteralEnergyCalculator.CalculateEnergyFromVolume(
                    EnteralVolume.Value,
                    comp);

            // ここで表示側kcalを同期したいなら（確認UIならやると分かりやすい）
            EnergyOrderValue = EnteralEnergy;

            // 成分計算は EnteralEnergy 確定後
            if (EnteralEnergy.HasValue)
            {
                EnteralProtein = EnteralEnergy.Value * comp.ProteinPerKcal;
                EnteralFat = EnteralEnergy.Value * comp.FatPerKcal;
                EnteralCarb = EnteralEnergy.Value * comp.CarbPerKcal;
                EnteralSalt = EnteralEnergy.Value * comp.SaltPerKcal;
                EnteralVitaminK = EnteralEnergy.Value * comp.VitaminKPerKcal;
                EnteralWater = EnteralEnergy.Value * comp.WaterPerKcal;
            }
        }
    }
}
        /*
        // ==============================
        // 経腸栄養 計算追加部分
        // ==============================
        if (!SelectedEnteralFormula.HasValue)
        {
            // 製剤未選択 → 何も計算しない
            return;
        }

        var formula = SelectedEnteralFormula.Value;
        var composition =
            EnteralFormulaTable.Get(formula);
       

        if (action == "volume")
        {
            // 投与量(mL) → kcal逆算
            TargetEnergyKcal =
                EnteralFeedingCalculator.CalculateKcal(
                    EnteralVolumeMl,
                    composition);
        }

        // 最終的には必ず kcal 基準で再計算
        EnteralResult =
            EnteralFeedingCalculator.CalculateComponents(
                TargetEnergyKcal,
                composition);

        // 表示用に量も同期
        EnteralVolumeMl = EnteralResult.VolumeMl;

        
        // kcal → mL
        double rawVolume =
            TargetEnergyKcal * composition.VolumePerKcal;

        // 規格量丸め
        EnteralVolumeMl =
            EnteralVolumeRounder.RoundUp(
                formula,
                rawVolume);

        // 丸め後 kcal 再計算
        TargetEnergyKcal =
            EnteralVolumeMl / composition.VolumePerKcal;


        // 規格量候補
        var packageSizes =
            EnteralPackageTable.Get(formula);

         Select 用リスト作成
        PackageVolumeOptions = packageSizes
            .OrderBy(x => x)
            .Select(x => new SelectListItem
            {
                Value = x.ToString(),
                Text = $"{x} mL"
            })
            .ToList();

        // 初回 or 未選択時：切り上げ規格量
        if (SelectedPackageVolume == 0)
        {
            SelectedPackageVolume =
                EnteralVolumeRounder.RoundUp(
                    formula,
                    rawVolume);
        }

        // 確定投与量
        EnteralVolumeMl = SelectedPackageVolume;

        // kcal を逆算
        TargetEnergyKcal =
            EnteralVolumeMl / composition.VolumePerKcal;

        // 疾患変更時のデフォルト反映
        if (!IsEnergyManuallyEdited)
        {
            SelectedEnergyOrder =
                EnergyOrderDefaultSelector.GetDefault(SelectedDisease);
        }

        // ① デフォルト選択（初回 or 疾患変更時）
        // ★ 初回 or 未選択時のみデフォルトを入れる
        if (SelectedEnergyOrder == EnergyOrderType.BmrEstimated)
        {
            SelectedEnergyOrder =
                EnergyOrderDefaultSelector.GetDefault(Disease);
        }

        // ② カロリー算出
        double calculatedEnergy =
            EnergyOrderValueTable.Calculate(
                SelectedEnergyOrder,
                EnergyRaw,                 // BMR等から算出済
                BodyIndex.StandardWeight);

        // ③ 手入力優先
        EnergyOrderValue =
            SelectedEnergyOrder == EnergyOrderType.Manual
                ? EnergyOrderValue
                : Math.Round(calculatedEnergy);

        //==============================
        // ⑩ 経腸栄養（Phase A）
        //==============================
        if (SelectedFormula.HasValue && EnergyOrderValue.HasValue)
        {
            var comp = EnteralFormulaTable.Get(SelectedFormula.Value);

            EnteralEnergy = EnergyOrderValue.Value;

            // 投与量（mL）
            EnteralVolume =
                EnteralEnergy * comp.VolumePerKcal;

            // 成分量
            EnteralProtein =
                EnteralEnergy * comp.ProteinPerKcal;

            EnteralFat =
                EnteralEnergy * comp.FatPerKcal;

            EnteralCarb =
                EnteralEnergy * comp.CarbPerKcal;

            EnteralSalt =
                EnteralEnergy * comp.SaltPerKcal;

            EnteralVitaminK =
                EnteralEnergy * comp.VitaminKPerKcal;

            EnteralWater =
                EnteralEnergy * comp.WaterPerKcal;
        }

        if (SelectedFormula.HasValue && EnergyOrderValue.HasValue)
        {
            var comp = EnteralFormulaTable.Get(SelectedFormula.Value);

            // kcal → mL
            if (!IsEnergyManuallyEdited)
            {
                var rawVolume = EnergyOrderValue.Value * comp.VolumePerKcal;

                EnteralVolume =
                    EnteralPackageRounder.RoundUp(
                        rawVolume,
                        SelectedPackage);

                EnteralEnergy =
                    EnteralEnergyCalculator.CalculateEnergyFromVolume(
                        EnteralVolume.Value,
                        comp);
            }
            // mL → kcal（手入力）
            else if (EnteralVolume.HasValue)
            {
                EnteralEnergy =
                    EnteralEnergyCalculator.CalculateEnergyFromVolume(
                        EnteralVolume.Value,
                        comp);

                EnergyOrderValue = EnteralEnergy;
            }
        }

    }
}


/*
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
    
    //[BindProperty] 
    //public ProteinCondition SelectedProteinCondition { get; set; }
    
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

    //==============================
    // 疾患・エネルギー関連
    //==============================
    // 疾患
    [BindProperty]
    public DiseaseType Disease { get; set; }

    // カロリー指示方法
    [BindProperty]
    public EnergyOrderType SelectedEnergyOrder { get; set; }

    // 手入力 kcal（Manual 用）
    [BindProperty]

    // 各方式の計算結果（表示用）
    public double EnergyBmrEstimated { get; private set; }
    public double Energy25 { get; private set; }
    public double Energy30 { get; private set; }
    public double Energy35 { get; private set; }

    // 最終オーダー
    public double EnergyFinal { get; private set; }
    public double? ManualEnergyKcal { get; set; }
    [BindProperty]
    public EnergyOrderType? SelectedEnergyLevel { get; set; }

    // 最終オーダーカロリー（手入力）
    [BindProperty]
    public int? OrderedEnergy { get; set; }

    // 表示用
    public int? CalculatedEnergy { get; private set; }
    public int? StandardWeightEnergy { get; private set; }
    public int? FinalEnergy { get; private set; }
    
    [BindProperty]
    public DiseaseType SelectedDisease { get; set; }

    [BindProperty]
    public EnergyOrderType SelectedEnergyOrder { get; set; }

    [BindProperty]
    public double? ManualEnergyValue { get; set; }

    // 表示用
    public double EnergyByBmr { get; private set; }
    public double Kcal25 { get; private set; }
    public double Kcal30 { get; private set; }
    public double Kcal35 { get; private set; }
    //public int EnergyDisplay { get; private set; }


    public void OnPost()
    {
        //==============================
        // 0. 蛋白補正の自動初期化
        //==============================
        var autoCorrection =
            ProteinCorrectionSelector.GetDefault(
                Age,
                SelectedDisease);

        // ユーザー未選択時のみ自動設定
        if (SelectedProteinCorrection == ProteinCorrectionType.None)
        {
            SelectedProteinCorrection = autoCorrection;
        }

        // 係数数値
        ProteinCorrect =
            SelectedProteinCorrection switch
            {
                ProteinCorrectionType.CKD3bTo5 => 0.7,
                ProteinCorrectionType.LiverCirrhosisPoor => 0.5,
                _ => 1.0
            };

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
                disease: SelectedDisease);

        double weightForProtein =
            WeightForCalculationSelector.Select(
                usage: WeightUsage.Protein,
                age: Age,
                actualWeight: Weight,
                adjustedWeight: AdjustedWeight,
                standardWeight: BodyIndex.StandardWeight,
                disease: SelectedDisease);

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

        // 疾患デフォルト
        SelectedEnergyOrder =
            DiseaseEnergyDefaultTable.Get(SelectedDisease);

        // kcal/kg 候補
        Kcal25 = 25 * BodyIndex.StandardWeight;
        Kcal30 = 30 * BodyIndex.StandardWeight;
        Kcal35 = 35 * BodyIndex.StandardWeight;
       
        */
/*
//==============================
// ② 成人疾患 → kcal/kg 自動設定
//==============================
if (Age >= 18)
{
    var defaultLevel =
        DiseaseEnergyLevelTable.GetDefault(SelectedDisease);

    // ユーザーが未選択なら自動設定
    if (!SelectedEnergyLevel.HasValue)
    {
        SelectedEnergyLevel = defaultLevel;
    }
}

//==============================
// ③ 標準体重 × kcal/kg
//==============================
if (Age >= 18 && BodyIndex?.StandardWeight != null && SelectedEnergyLevel.HasValue)
{
    StandardWeightEnergy =
        (int)Math.Round(
            BodyIndex.StandardWeight
            * (int)SelectedEnergyLevel.Value,
            MidpointRounding.AwayFromZero);
}

//==============================
// ④ 最終オーダーカロリー
//==============================
FinalEnergy =
    OrderedEnergy
    ?? StandardWeightEnergy
    ?? CalculatedEnergy;
*/


/*
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
    
}
*/
