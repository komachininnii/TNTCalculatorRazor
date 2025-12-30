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


    // =====================================
    // Action 判定・ModelState ヘルパ（追加）
    // =====================================

    private string Act => (Action ?? "").Trim().ToLowerInvariant();

    // kcal or mL をユーザーが触ったとみなす Action
    private static readonly HashSet<string> EnergyEditActions =
        new(StringComparer.OrdinalIgnoreCase) { "energy", "volume" };

    // 必要量→投与カロリーへ同期する対象 Action
    private static readonly HashSet<string> ShouldSyncActions =
        new(StringComparer.OrdinalIgnoreCase) { "anthro", "disease", "order", "factors", "protein" };

    private bool IsEnergyEditAction( string act ) => EnergyEditActions.Contains(act);
    private bool ShouldSyncEnergyOrder( string act ) => ShouldSyncActions.Contains(act);

    private void ClearModelState( params string[] keys )
    {
        foreach (var key in keys)
            ModelState.Remove(key);
    }

    // 蛋白補正をユーザーが手動で変更したか（疾患変更時の自動追従を制御）
    [BindProperty]
    public bool IsProteinCorrectionUserEdited { get; set; }

    // 肝性脳症（肝硬変のときだけUI表示）
    [BindProperty]
    public bool IsHepaticEncephalopathy { get; set; }




    // ==============================
    // POST
    // ==============================
    public void OnPost()
    {
        var act = Act;

        // 0) ユーザー編集フラグ（kcal or mL を触ったら以後自動同期しない）
        if (IsEnergyEditAction(act))
            IsEnergyUserEdited = true;

        // 蛋白補正を手で触ったら以後はデフォルト上書きをしない
        // （肝性脳症チェックは “状態入力” 扱いなのでここでは true にしない）
        if (act == "protein")
            IsProteinCorrectionUserEdited = true;

        // 1) 基本計算（入力が揃っていて範囲内なら BMR/標準体重などが埋まる）
        RecalcBase();

        // 小児は「例外疾患の対象外」：疾患は None に固定（UIもdisabled化する想定）
        if (Age.HasValue && Age.Value < 18)
        {
            if (SelectedDisease != DiseaseType.None)
            {
                SelectedDisease = DiseaseType.None;
                ClearModelState(nameof(SelectedDisease));
            }
            
            if (IsHepaticEncephalopathy)
            {
                IsHepaticEncephalopathy = false;
                ClearModelState(nameof(IsHepaticEncephalopathy));
            }
        }

        // ★ 疾患から透析フラグを同期（UIにチェックが無くても一致させる）
        IsHemodialysis = (SelectedDisease == DiseaseType.Hemodialysis);

        // 2) 疾患 → 算出方法（デフォルト）へ切替（ただし手動編集後は尊重）
        if (act == "disease" && !IsEnergyUserEdited)
        {
            SelectedEnergyOrder = EnergyOrderDefaultSelector.GetDefault(SelectedDisease);
            ClearModelState(nameof(SelectedEnergyOrder)); // UIのselectに反映（ModelState優先対策）
        }
                
        // 肝硬変以外を選んだら肝性脳症チェックは解除
        if (act == "disease" && SelectedDisease != DiseaseType.LiverCirrhosis && IsHepaticEncephalopathy)
        {
            IsHepaticEncephalopathy = false;
            ClearModelState(nameof(IsHepaticEncephalopathy));
        }
        
        // 3) 蛋白補正のデフォルト（年齢が入っているときだけ）
        //    - 疾患/身体入力が変わった時は、手動編集していない限りデフォルトへ追従
        //    - 肝硬変＋肝性脳症チェックONは 0.5 を強制（安全側）
        if (Age.HasValue)
        {
            // 肝硬変＋肝性脳症：0.5補正（ユーザー手動より優先）
            if (SelectedDisease == DiseaseType.LiverCirrhosis && IsHepaticEncephalopathy)
            {
                if (SelectedProteinCorrection != ProteinCorrectionType.LiverCirrhosisPoor)
                {
                    SelectedProteinCorrection = ProteinCorrectionType.LiverCirrhosisPoor;
                    ClearModelState(nameof(SelectedProteinCorrection));
                }
            }
            else
            {
                // 通常：手動編集していない場合のみデフォルト追従
                if (!IsProteinCorrectionUserEdited && (act == "disease" || act == "anthro" || act == "hepatic"))
                {
                    SelectedProteinCorrection =
                        ProteinCorrectionSelector.GetDefault(Age.Value, SelectedDisease);
                    ClearModelState(nameof(SelectedProteinCorrection));
                }
            }
        }

        // 4) 係数・補正込みの必要量を計算（EnergyFinal / Protein / Water など）
        RecalcEnergyProteinWater();

        // 5) 必要量（EnergyFinal）→ 経腸の投与カロリーへ同期
        //    ※ ユーザーが編集していないときのみ
        if (ShouldSyncEnergyOrder(act) && !IsEnergyUserEdited)
        {
            SyncEnergyOrderValueFromNeedOrFallback();
        }

        // 6) 経腸栄養（kcal↔mL 同期、成分、割付候補）
        RecalcEnteral();
    }


    // ==============================
    // 5) 同期処理をメソッド化（追加）
    // ==============================
    private void SyncEnergyOrderValueFromNeedOrFallback()
    {
        // (A) 原則：EnergyFinal があれば最優先で採用
        if (EnergyFinal.HasValue)
        {
            EnergyOrderValue = EnergyFinal.Value;
            ClearModelState(nameof(EnergyOrderValue), nameof(EnteralVolumeInput)); // mL欄も追従させたい場合
            return;
        }

        // (B) フォールバック：EnergyFinalが作れない時だけ、候補（BMR/25/30/35）から入れる
        EnergyOrderValue = SelectedEnergyOrder switch
        {
            EnergyOrderType.BmrEstimated => BmrKcal,
            EnergyOrderType.Kcal25 => Kcal25,
            EnergyOrderType.Kcal30 => Kcal30,
            EnergyOrderType.Kcal35 => Kcal35,
            _ => EnergyOrderValue
        };

        ClearModelState(nameof(EnergyOrderValue));
    }


    // ==============================
    // 基本計算
    // ==============================
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


    // ==============================
    // エネルギー/蛋白/水分（参考表示）
    // ==============================
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

        // 蛋白
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
                proteinCorrect,
                SelectedDisease);

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


    // ==============================
    // 経腸栄養（必要量ベース + 割付候補）
    // ==============================
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

        var act = Act;

        if (!SelectedEnteralFormula.HasValue)
        {
            EnteralVolumeInput = null;
            ClearModelState(nameof(EnteralVolumeInput));
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
            ClearModelState(nameof(EnergyOrderValue));

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
            ClearModelState(nameof(EnteralVolumeInput));

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
            ClearModelState(nameof(EnteralVolumeInput));
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

//==============================
// POST
//==============================
public void OnPost()
{
   var act = (Action ?? "").ToLowerInvariant();

    // 0) ユーザー編集フラグ（kcal or mL を触ったら以後自動同期しない）
    if (act == "energy" || act == "volume")
        IsEnergyUserEdited = true;

    // 1) 基本計算（入力が揃っていて範囲内なら BMR/標準体重などが埋まる）
    RecalcBase();

    // ★ 疾患から透析フラグを同期（UIにチェックが無くても一致させる）
    IsHemodialysis = (SelectedDisease == DiseaseType.Hemodialysis);

    // 2) 疾患 → 算出方法（デフォルト）へ切替（ただし手動編集後は尊重）
    if (act == "disease" && !IsEnergyUserEdited)
    {
        SelectedEnergyOrder = EnergyOrderDefaultSelector.GetDefault(SelectedDisease);
        ModelState.Remove(nameof(SelectedEnergyOrder)); // UIのselectに反映
    }

    // 3) 蛋白補正のデフォルト（年齢が入っているときだけ）
    //    - ユーザーが手動選択していない（Noneのまま）場合のみ自動決定
    if (SelectedProteinCorrection == ProteinCorrectionType.None && Age.HasValue)
    {
        SelectedProteinCorrection =
            ProteinCorrectionSelector.GetDefault(Age.Value, SelectedDisease);

        // selectに反映（ModelState優先対策）
        ModelState.Remove(nameof(SelectedProteinCorrection));
    }

    // 4) 係数・補正込みの必要量を計算（EnergyFinal / Protein / Water など）
    RecalcEnergyProteinWater();

    // 5) 必要量（EnergyFinal）→ 経腸の投与カロリーへ同期
    //    ※ ユーザーが編集していないときのみ
    var shouldSync =
        act == "anthro" || act == "disease" || act == "order" || act == "factors" || act == "protein";

    if (shouldSync && !IsEnergyUserEdited)
    {
        // (A) 原則：EnergyFinal があれば最優先で採用
        if (EnergyFinal.HasValue)
        {
            EnergyOrderValue = EnergyFinal.Value;
            ModelState.Remove(nameof(EnergyOrderValue));
            ModelState.Remove(nameof(EnteralVolumeInput)); // mL欄も追従させたい場合
        }
        else
        {
            // (B) フォールバック：EnergyFinalが作れない時だけ、候補（BMR/25/30/35）から入れる
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
    }

    // 6) 経腸栄養（kcal?mL 同期、成分、割付候補）
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

    // 蛋白
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
            proteinCorrect,
            SelectedDisease);

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

*/