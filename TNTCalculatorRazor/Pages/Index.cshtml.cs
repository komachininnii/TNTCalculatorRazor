using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;
using TNTCalculatorRazor.Domain.Calculators;
using TNTCalculatorRazor.Domain.Enums;
using TNTCalculatorRazor.Domain.Results;
using TNTCalculatorRazor.Domain.Rules;
using TNTCalculatorRazor.Domain.Selectors;
using TNTCalculatorRazor.Domain.Services;
using TNTCalculatorRazor.Domain.Tables;
using Microsoft.Extensions.Options;
using TNTCalculatorRazor.Domain.Constants;
using TNTCalculatorRazor.Domain.Models;

public class IndexModel : PageModel
{
    public string AppVersion
    {
        get
        {
            var info =
                Assembly.GetExecutingAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                    .InformationalVersion;

            if (!string.IsNullOrWhiteSpace(info))
            {
                // 例: "0.9.0-beta.1+abcdef..." → "0.9.0-beta.1"
                var cut = info.Split('+')[0];
                return cut;
            }

            // フォールバック（数値版しかない場合）
            var v = Assembly.GetExecutingAssembly().GetName().Version;
            if (v is null) return "";
            return $"{v.Major}.{v.Minor}.{v.Build}";
        }
    }
    
    // コンストラクタ
    public IndexModel( IOptions<InternalManualOptions> manualOptions )
    {
        var m = manualOptions.Value;
        ShowInternalManualLink = m.Enabled && !string.IsNullOrWhiteSpace(m.Url);
        InternalManualUrl = m.Url;
    }

    //==============================
    // システム / デバッグ
    //==============================

    // 内部マニュアルリンク
    public bool ShowInternalManualLink { get; private set; }
    public string? InternalManualUrl { get; private set; }

    // デバッグ用：体重算出内訳表示
    public string DebugWeightLine { get; private set; } = "";

    //==============================
    // 入力（BindProperty）
    //==============================

    // 基本情報
    [BindProperty] public int? Age { get; set; }                 // 年齢（年）null=未入力, 0=乳児
    [BindProperty] public double? Height { get; set; }           // cm
    [BindProperty] public double? Weight { get; set; }           // kg
    [BindProperty] public GenderType Gender { get; set; } = GenderType.Male;
    [BindProperty] public bool IsPregnant { get; set; }

    // 係数・状態
    [BindProperty] public ActivityFactorType ActivityFactor { get; set; } = ActivityFactorType.BedriddenComa;
    [BindProperty] public StressFactorType StressFactor { get; set; } = StressFactorType.Normal;
    [BindProperty] public BodyTemperatureLevel SelectedBodyTemperature { get; set; } = BodyTemperatureLevel.Normal;
    [BindProperty] public PressureUlcerLevel SelectedPressureUlcer { get; set; } = PressureUlcerLevel.None;

    // 疾患・補正
    [BindProperty] public DiseaseType SelectedDisease { get; set; } = DiseaseType.None;
    [BindProperty] public ProteinCorrectionType SelectedProteinCorrection { get; set; } = ProteinCorrectionType.None;
    [BindProperty] public bool IsProteinCorrectionUserEdited { get; set; }

    // 肝性脳症（肝硬変時のみ有効）
    [BindProperty] public bool IsHepaticEncephalopathy { get; set; }

    // エネルギー指示
    [BindProperty] public EnergyOrderType SelectedEnergyOrder { get; set; } = EnergyOrderType.CorrectedBmrBased;
    [BindProperty] public int? ManualEnergyValue { get; set; }
    [BindProperty] public int? EnergyOrderValue { get; set; }
    [BindProperty] public bool IsEnergyUserEdited { get; set; }

    // 透析・その他
    [BindProperty] public bool IsHemodialysis { get; set; }

    // 経腸栄養（入力）
    [BindProperty] public EnteralFormulaType? SelectedEnteralFormula { get; set; }
    [BindProperty] public int? EnteralVolumeInput { get; set; }
    [BindProperty] public string? Action { get; set; }

    // ★ 未使用 → 削除候補
    // [BindProperty] public bool HasUserSelectedPackage { get; set; }

    //==============================
    // 基礎代謝・体格（Raw / Display）
    //==============================

    // 実測体重BMR（Raw）
    public double? ActualBmrRaw { get; private set; }
    public BmrFormulaType? ActualBmrFormula { get; private set; }

    // 表示用（Actual BMR）
    public int? ActualBmrDisplayKcal =>
        ActualBmrRaw.HasValue
            ? (int?)RoundingRules.RoundKcalToInt(ActualBmrRaw.Value)
            : null;

    public string? ActualBmrFormulaDisplay { get; private set; }
    public string? ActualBmrFormulaDisplayLong { get; private set; }

    //==============================
    // 体重・体格指数
    //==============================

    public BodyIndexResult? BodyIndex { get; private set; }
    public double? BodySurfaceArea { get; private set; }

    // 補正体重
    public double? AdjustedWeight { get; private set; }
    public double? CorrectedWeight { get; private set; }

    // Corrected BMR に用いた体重情報（UI表示用）
    public BmrWeightBasisType? CorrectedBmrWeightBasis { get; private set; }
    public double? CorrectedBmrWeightUsed { get; private set; }

    //==============================
    // ストレス係数
    //==============================

    public double StressBase { get; private set; }
    public double StressTemperature { get; private set; }
    public double StressPressureUlcer { get; private set; }
    public double StressTotal { get; private set; }

    //==============================
    // エネルギー計算
    //==============================

    // 候補（表示用）
    public int? Kcal25 { get; private set; }
    public int? Kcal30 { get; private set; }
    public int? Kcal35 { get; private set; }

    // Corrected BMR × 係数（参考表示）
    public int? CorrectedBmrEnergyDisplayKcal { get; private set; }

    // 最終仕様確定値
    public int? EnergyFinal { get; private set; }

    //==============================
    // 蛋白・水分
    //==============================

    // 蛋白
    public double? ProteinRaw { get; private set; }
    public double? ProteinFinal { get; private set; }

    public string? ProteinDisplayText =>
        ProteinFinal.HasValue ? ProteinFinal.Value.ToString("F1") : null;

    // 水分
    public double? WaterRaw { get; private set; }
    public int? WaterFinal { get; private set; }
    public bool WaterFeverCorrected { get; private set; }

    public string? WaterDisplay =>
        WaterFinal.HasValue ? WaterFinal.Value.ToString() : null;

    //==============================
    // 経腸栄養
    //==============================

    public int? EnteralVolume { get; private set; }          // mL/day（仕様確定）
    public double? EnteralEnergy { get; private set; }       // kcal/day（表示）

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
    // 推定CCr
    //==============================

    [BindProperty] public double? SerumCreatinine { get; set; }

    public double? CcrActual { get; private set; }
    public double? CcrCorrected { get; private set; }
    public string CcrNote { get; private set; } = "";

    //==============================
    // Action / ヘルパ
    //==============================

    private string Act => (Action ?? "").Trim().ToLowerInvariant();

    // ユーザー手動入力の判定
    private static readonly HashSet<string> EnergyEditActions =
        new(StringComparer.OrdinalIgnoreCase) { "energy", "volume" };

    private bool IsEnergyEditAction( string act ) => EnergyEditActions.Contains(act);

    private void ClearModelState( params string[] keys )
    {
        foreach (var key in keys)
            ModelState.Remove(key);
    }


    // ==============================
    // CCr計算
    // ==============================
    private void RecalcCcr( bool addErrors )
    {
        CcrActual = null;
        CcrCorrected = null;
        CcrNote = "";

        // 必須：年齢・体重・性別・Cr
        if (!Age.HasValue || !Weight.HasValue) return;

        if (!SerumCreatinine.HasValue)
        {
            // 未入力は（ボタンなし運用なら）沈黙でOK
            return;
        }

        // Cr
        if (SerumCreatinine.Value <= 0)
        {
            if (addErrors)
                ModelState.AddModelError(nameof(SerumCreatinine), "Crは0より大きい値で入力してください。");
            return;
        }

        // あり得ない値（renal のときだけ教える）
        if (Age.Value < InputConstraints.AgeMin || Age.Value > InputConstraints.AgeMax)
        {
            if (addErrors)
                ModelState.AddModelError(
                    nameof(Age),
                    $"年齢は {InputConstraints.AgeMin}〜{InputConstraints.AgeMax} の範囲で入力してください。"
                );
            return;
        }
        if (Weight.Value < InputConstraints.WeightMin || Weight.Value > InputConstraints.WeightMax)
        {
            if (addErrors)
                ModelState.AddModelError(
                    nameof(Weight),
                    $"体重は {InputConstraints.WeightMin:0.0}〜{InputConstraints.WeightMax:0.0} kg の範囲で入力してください。"
                );
            return;
        }

        var cr = SerumCreatinine.Value;

        CcrActual = CcrCalculator.Calculate(Age.Value, Weight.Value, cr, Gender);

        var correctedCr = CcrCreatinineCorrectionRule.GetCorrectedCreatinine(Age.Value, Gender, cr);
        if (correctedCr.HasValue)
        {
            CcrCorrected = CcrCalculator.Calculate(Age.Value, Weight.Value, correctedCr.Value, Gender);

            var type = CcrCreatinineCorrectionRule.GetType(Age.Value, Gender, cr);
            CcrNote = CcrCreatinineCorrectionRule.GetNote(type);
        }
    }


    // ==============================
    // POST
    // ==============================
    public void OnPost()
    {
        RecalcAll();
    }

    public PartialViewResult OnPostRecalc()
    {
        RecalcAll();
        return Partial("_ResultPanel", this);
    }

    private void RecalcAll()
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
        RecalcBase(addErrors: Act == "anthro" || act == "renal");

        // 1.5) 腎機能CCr
        RecalcCcr(addErrors: act == "anthro" || act == "renal");

        // 小児は「例外疾患の対象外」：疾患は None に固定（UIもdisabled化する想定）
        if (Age.HasValue && Age.Value < 18)
        {
            var forcedDiseaseReset = false;
            
            // 疾患を None に強制
            if (SelectedDisease != DiseaseType.None)
            {
                SelectedDisease = DiseaseType.None;
                ClearModelState(nameof(SelectedDisease));
                forcedDiseaseReset = true;
            }
            // 肝性脳症チェックも解除
            if (IsHepaticEncephalopathy)
            {
                IsHepaticEncephalopathy = false;
                ClearModelState(nameof(IsHepaticEncephalopathy));
            }
            // エネルギー算出方法もデフォルトへ強制
            if (forcedDiseaseReset && !IsEnergyUserEdited && act == "anthro")
            {
                SelectedEnergyOrder = EnergyOrderDefaultsTable.GetDefault(SelectedDisease);
                ClearModelState(nameof(SelectedEnergyOrder));
            }
        }

        // ★ 疾患から透析フラグを同期（UIにチェックが無くても一致させる）
        IsHemodialysis = (SelectedDisease == DiseaseType.Hemodialysis);

        // 2) 疾患 → 算出方法（デフォルト）へ切替（ただし手動編集後は尊重）
        if (act == "disease" && !IsEnergyUserEdited)
        {
            SelectedEnergyOrder = EnergyOrderDefaultsTable.GetDefault(SelectedDisease);
            ClearModelState(nameof(SelectedEnergyOrder)); // UIのselectに反映（ModelState優先対策）
        }
                
        // 肝硬変以外を選んだら肝性脳症チェックは解除
        if (act == "disease" && SelectedDisease != DiseaseType.LiverCirrhosis && IsHepaticEncephalopathy)
        {
            IsHepaticEncephalopathy = false;
            ClearModelState(nameof(IsHepaticEncephalopathy));
        }

        // ★ 蛋白補正：None(なし)に戻したら自動追従へ復帰させる
        // （proteinアクションのときだけ、手動編集フラグを整合させる）
        if (act == "protein")
        {
            IsProteinCorrectionUserEdited = (SelectedProteinCorrection != ProteinCorrectionType.None);
            ClearModelState(nameof(IsProteinCorrectionUserEdited));
        }

        // 3) 蛋白補正のデフォルト（年齢が入っているときだけ）
        //    - 疾患/身体入力が変わった時は、手動編集していない限りデフォルトへ追従
        //    - 肝硬変＋肝性脳症チェックONは 0.5 を強制
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

        // 5) 必要量（EnergyFinal）→ EnergyOrderValue に同期（ユーザー手動編集していないときのみ）
        if (!IsEnergyUserEdited)
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

        // (B) フォールバック：EnergyFinalが作れない時だけ、候補（補正BMRx係数/25/30/35）から入れる
        EnergyOrderValue = SelectedEnergyOrder switch
        {
            EnergyOrderType.CorrectedBmrBased => CorrectedBmrEnergyDisplayKcal,
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
    private bool CanCalcBase( bool addErrors )
    {
        // 必須：年齢・身長・体重
        if (!Age.HasValue || !Height.HasValue || !Weight.HasValue)
            return false;

        bool isValid = true;

        // 範囲（「あり得ない値」を弾く）
        if (Age.Value < InputConstraints.AgeMin || Age.Value > InputConstraints.AgeMax)
        {
            if (addErrors)
            {
                ModelState.AddModelError(
                    nameof(Age),
                    $"年齢は {InputConstraints.AgeMin}〜{InputConstraints.AgeMax} の範囲で入力してください。"
                );
            }
            isValid = false;
        }
        if (Height.Value < InputConstraints.HeightMin || Height.Value > InputConstraints.HeightMax)
        {
            if (addErrors)
            {
                ModelState.AddModelError(
                    nameof(Height),
                    $"身長は {InputConstraints.HeightMin:0.0}〜{InputConstraints.HeightMax:0.0} cm の範囲で入力してください。"
                );
            }
            isValid = false;
        }
        if (Weight.Value < InputConstraints.WeightMin || Weight.Value > InputConstraints.WeightMax)
        {
            if (addErrors)
            {
                ModelState.AddModelError(
                    nameof(Weight),
                    $"体重は {InputConstraints.WeightMin:0.0}〜{InputConstraints.WeightMax:0.0} kg の範囲で入力してください。"
                );
            }
            isValid = false;
        }

        return isValid;
    }

    private void RecalcBase( bool addErrors )
    {
        // 初期化
        ActualBmrRaw = null;
        ActualBmrFormula = null;
        ActualBmrFormulaDisplay = "";
        ActualBmrFormulaDisplayLong = "";

        BodyIndex = null;
        BodySurfaceArea = null;
        AdjustedWeight = null;
        CorrectedWeight = null;

        CorrectedBmrWeightBasis = null;
        CorrectedBmrWeightUsed = null;

        Kcal25 = Kcal30 = Kcal35 = null;
       
        if (!CanCalcBase(addErrors))
            return;

        if (Gender != GenderType.Female || !Age.HasValue || Age.Value < 18 || Age.Value > 55)
            IsPregnant = false;


        // BMR / 体格 / BSA
        // 実測体重BMR
        var actualBmr = BmrCalculator.Calculate(Age!.Value, Weight!.Value, Height!.Value, Gender);
        ActualBmrRaw = actualBmr.RawValue;
        ActualBmrFormula = actualBmr.Formula;

        ActualBmrFormulaDisplay = ActualBmrFormula?.ToShortName() ?? "";
        ActualBmrFormulaDisplayLong = ActualBmrFormula?.ToLongName() ?? "";

        BodyIndex = BodyIndexCalculator.Calculate(Age.Value, Height.Value, Weight.Value, Gender);
        BodySurfaceArea = BodySurfaceAreaCalculator.Calculate(Height.Value, Weight.Value);

       
        // ※ BodyIndex が計算済みである前提
        double obesityDegree = BodyIndex?.ObesityDegree ?? 0;

        // 採用体重のpill表示用
        CorrectedBmrWeightBasis = CorrectedWeightCalculator.GetBasis(Age.Value, obesityDegree);

        // BodyIndex はこの時点で存在する前提（CanCalcBase通過＋Calculate済み）
        var bodyIndex = BodyIndex!;

        // 調整体重を保持
        AdjustedWeight = CorrectedWeightCalculator.CalculateAdjustedWeight(Weight.Value, bodyIndex.StandardWeight);
                
        // 補正体重（BMR/エネルギー/蛋白などの基礎として使う“最終採用体重”）
        CorrectedWeight =
            CorrectedWeightCalculator.CalculateCorrectedWeight(
                Age.Value,
                Weight.Value,
                bodyIndex.StandardWeight,
                obesityDegree);

        // 体重補正BMRで用いる体重値
        CorrectedBmrWeightUsed = CorrectedWeight.Value;

        // フォールバック用：CorrectedBMR（係数1想定）
        var correctedBmrRaw =
        BmrCalculator.Calculate(Age.Value, CorrectedWeight.Value, Height.Value, Gender)
                   .RawValue;

        // 25/30/35 は標準体重ベース
        // ※ StandardWeight が計算できている前提
        Kcal25 = RoundingRules.RoundKcalToInt(bodyIndex.StandardWeight * 25.0);
        Kcal30 = RoundingRules.RoundKcalToInt(bodyIndex.StandardWeight * 30.0);
        Kcal35 = RoundingRules.RoundKcalToInt(bodyIndex.StandardWeight * 35.0);
    }


    // ==============================
    // エネルギー/蛋白/水分（参考表示）
    // ==============================
    private void RecalcEnergyProteinWater()
    {
        EnergyFinal = null;
        CorrectedBmrEnergyDisplayKcal = null;
        ProteinRaw = null;
        ProteinFinal = null;
        WaterFeverCorrected = false;

        // ストレスは入力が揃わなくても計算可能
        StressBase = StressFactorTable.Get(StressFactor);
        StressTemperature = TemperatureStressTable.GetAddition(SelectedBodyTemperature);
        StressPressureUlcer = PressureUlcerStressTable.GetAddition(SelectedPressureUlcer);
        StressTotal = StressBase + StressTemperature + StressPressureUlcer;

        if (!CanCalcBase(addErrors: false) || BodyIndex is null || CorrectedWeight is null)
            return;

        var correctedBmrRaw =
            BmrCalculator.Calculate(Age!.Value, CorrectedWeight.Value, Height!.Value, Gender)
                .RawValue;

        var correctedBmrEnergyRawKcal =
            Age.Value == 0
                   ? ((correctedBmrRaw * StressTotal) + (40 * Weight!.Value)) * 1.1
                   : correctedBmrRaw
                        *ActivityFactorTable.Get(ActivityFactor)
                        *StressTotal;

        // 参考表示用（体重補正代謝量 × 係数）
        CorrectedBmrEnergyDisplayKcal =
            RoundingRules.RoundKcalToInt(correctedBmrEnergyRawKcal);

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
                EnergyOrderType.Manual => ManualEnergyValue ?? correctedBmrEnergyRawKcal,
                _ => correctedBmrEnergyRawKcal
            };

        // 最終エネルギー必要量を仕様整数化
        EnergyFinal = RoundingRules.RoundKcalToInt(selected);

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
                Age!.Value,
                Weight!.Value,
                CorrectedWeight.Value,
                BodyIndex.StandardWeight,
                SelectedDisease);

        ProteinRaw =
            ProteinCalculator.Calculate(
                Age.Value,
                weightForProtein,
                StressTotal,
                proteinCorrect,
                SelectedDisease);

        // 最終蛋白値
        ProteinFinal =
            RoundingRules.RoundGram1dp(ProteinRaw.Value);

        // 水分
        WaterRaw =
            WaterCalculator.CalculateTotal(
                Age.Value,
                Weight.Value,
                IsHemodialysis,
                IsPregnant,
                AdjustedWeight!.Value,          // 妊娠で肥満度120%以上なら調整体重
                BodyIndex.ObesityDegree ?? 0,
                SelectedBodyTemperature);

        // 水分最終値
        WaterFinal = RoundingRules.CeilMl(WaterRaw.Value);

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
            EnteralEnergy =
                EnteralEnergyCalculator.CalculateEnergyFromVolume(EnteralVolume.Value, comp);

            // kcal入力欄の同期（仕様丸め）
            EnergyOrderValue =
                RoundingRules.RoundKcalToInt(EnteralEnergy.Value);

            ClearModelState(nameof(EnergyOrderValue));

            // 割付候補
            EnteralPackagePlans =
                EnteralPackageAllocator.BuildPlans(
                    RoundingRules.RoundEnteralMl(EnteralVolume.Value),
                    packageVolumes.ToList(),
                    maxPlans: maxToShow);

        }
        else if (EnergyOrderValue.HasValue && EnergyOrderValue.Value > 0)
        {
            var targetKcal = EnergyOrderValue.Value;

            // kcal → 必要mL（Raw）
            var rawVolume = targetKcal * comp.VolumePerKcal;
            
            // 仕様丸め後の mL
            EnteralVolume = RoundingRules.RoundEnteralMl(rawVolume);

            // 入力欄も同期
            EnteralVolumeInput = EnteralVolume;
            ClearModelState(nameof(EnteralVolumeInput));

            // 表示投与量ベースの kcal
            EnteralEnergy = EnteralEnergyCalculator.CalculateEnergyFromVolume(EnteralVolume.Value, comp);

            // 割付候補
            EnteralPackagePlans =
                EnteralPackageAllocator.BuildPlans(
                    //targetVolumeMl,
                    EnteralVolume.Value,
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
