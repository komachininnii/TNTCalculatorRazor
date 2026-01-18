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

    // 内部マニュアルリンク
    public bool ShowInternalManualLink { get; private set; }
    public string? InternalManualUrl { get; private set; }
    
    // デバッグ用：体重算出内訳表示
    public string DebugWeightLine { get; private set; } = "";


    //==============================
    // 入力（Bind）
    //==============================
    // 基本入力（空欄スタート）
    [BindProperty] public int? Age { get; set; }                 // 年齢（年） null=未入力, 0=乳児
    [BindProperty] public double? Height { get; set; }           // 身長（cm） null=未入力
    [BindProperty] public double? Weight { get; set; }           // 体重（kg） null=未入力
    [BindProperty] public GenderType Gender { get; set; } = GenderType.Male;
    [BindProperty] public bool IsPregnant { get; set; } = false;

    [BindProperty] public ActivityFactorType ActivityFactor { get; set; } = ActivityFactorType.BedriddenComa;
    [BindProperty] public StressFactorType StressFactor { get; set; } = StressFactorType.Normal;
    [BindProperty] public BodyTemperatureLevel SelectedBodyTemperature { get; set; } = BodyTemperatureLevel.Normal;
    
    // 褥瘡（ストレス加算のみ）
    [BindProperty]
    public PressureUlcerLevel SelectedPressureUlcer { get; set; } = PressureUlcerLevel.None;
    // 表示用（内訳）
    public double StressPressureUlcer { get; private set; }

    [BindProperty] public DiseaseType SelectedDisease { get; set; } = DiseaseType.None;
    [BindProperty] public ProteinCorrectionType SelectedProteinCorrection { get; set; } = ProteinCorrectionType.None;

    // BMRで用いた式（表示用）
    public string BmrFormulaDisplay { get; private set; } = "";
    public string BmrFormulaDisplayLong { get; private set; } = "";
    // エネルギー算出方法（疾患でデフォルト切替）
    [BindProperty] public EnergyOrderType SelectedEnergyOrder { get; set; } = EnergyOrderType.BmrEstimated;

    [BindProperty] public int? ManualEnergyValue { get; set; }

    // 経腸栄養側の編集可能な「投与カロリー」
    [BindProperty] public int? EnergyOrderValue { get; set; }

    // ユーザーが投与カロリー/投与量を手で触ったか（疾患デフォルト上書き防止）
    [BindProperty] public bool IsEnergyUserEdited { get; set; } = false;

    [BindProperty] public bool IsHemodialysis { get; set; }
    
    // 経腸栄養
    [BindProperty] public EnteralFormulaType? SelectedEnteralFormula { get; set; }
    [BindProperty] public int? EnteralVolumeInput { get; set; }  // mL/day 手入力（端数調整用）
    [BindProperty] public string? Action { get; set; }           // hidden
    [BindProperty] public bool HasUserSelectedPackage { get; set; }

    //==============================
    // 計算結果（表示用）
    //==============================
    public BmrResult? BmrResult { get; private set; }
    // 基礎エネルギー算出に用いた体重の基準
    public BmrWeightBasisType? BmrWeightBasis { get; private set; }
    // 基礎エネルギー算出に用いた体重の値
    public double? BmrWeightUsed { get; private set; }

    public BodyIndexResult? BodyIndex { get; private set; }

    public double? AdjustedWeight { get; private set; }
    // BMRに最終的に採用された体重（実測/標準/調整のいずれか）
    //public double? BmrWeightFinal { get; private set; }

    // 計算に用いる補正体重（肥満度で 実測/標準/調整 を切替した“最終採用体重”）
    public double? CorrectedWeight { get; private set; }

    // BMR表示文脈で読みやすいエイリアス（中身は同じ）
    public double? BmrWeightFinal => CorrectedWeight;

    public double? BodySurfaceArea { get; private set; }

    // ストレス内訳
    public double StressBase { get; private set; }
    public double StressTemperature { get; private set; }
    public double StressTotal { get; private set; }

    // エネルギー候補（表示用）
    public int? BmrKcal { get; private set; }
    // 参考：基礎エネルギー×係数（BMR推定ルートの最終値）
    public int? EnergyByBmrKcal { get; private set; }
    public int? Kcal25 { get; private set; }
    public int? Kcal30 { get; private set; }
    public int? Kcal35 { get; private set; }

    // 計算した最終値（参考表示用）
    public int? EnergyFinal { get; private set; }                // SelectedEnergyOrder + Manual の結果
    public double? ProteinRaw { get; private set; }
    public string? ProteinDisplayText { get; private set; }
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


    // Creatinine追加入力（任意）
    [BindProperty]
    public double? SerumCreatinine { get; set; }  // mg/dL

    // CCr（参考表示）
    public double? CcrActual { get; private set; }
    public double? CcrCorrected { get; private set; }
    public string CcrNote { get; private set; } = "";


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
    // CCr計算
    // ==============================
    private void RecalcCcr()
    {
        CcrActual = null;
        CcrCorrected = null;
        CcrNote = "";

        // 必須：年齢・体重・性別・Cr
        if (!Age.HasValue || !Weight.HasValue) return;
        if (!SerumCreatinine.HasValue || SerumCreatinine.Value <= 0) return;

        // あり得ない値は計算しない（エラー表示は今はしない方針）
        if (Age.Value < 0 || Age.Value >= 130) return;
        if (Weight.Value < 0.5 || Weight.Value >= 300) return;

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
        var act = Act;

        // 0) ユーザー編集フラグ（kcal or mL を触ったら以後自動同期しない）
        if (IsEnergyEditAction(act))
            IsEnergyUserEdited = true;

        // 蛋白補正を手で触ったら以後はデフォルト上書きをしない
        // （肝性脳症チェックは “状態入力” 扱いなのでここでは true にしない）
        if (act == "protein")
            IsProteinCorrectionUserEdited = true;

        // 1) 基本計算（入力が揃っていて範囲内なら BMR/標準体重などが埋まる）
        RecalcBase(addErrors: Act == "anthro");

        // 1.5) 腎機能（参考）CCr
        RecalcCcr();

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

        // ★ 蛋白補正：None(なし)に戻したら自動追従へ復帰させる
        // （proteinアクションのときだけ、手動編集フラグを整合させる）
        if (act == "protein")
        {
            IsProteinCorrectionUserEdited = (SelectedProteinCorrection != ProteinCorrectionType.None);
            ClearModelState(nameof(IsProteinCorrectionUserEdited));
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
    private bool CanCalcBase( bool addErrors )
    {
        // 必須：年齢・身長・体重
        if (!Age.HasValue || !Height.HasValue || !Weight.HasValue)
            return false;

        // 範囲（「あり得ない値」を弾く）
        if (Age.Value < 0 || Age.Value >= 130)
        {
            ModelState.AddModelError(nameof(Age), "年齢は 0〜129 の範囲で入力してください。");
            return false;
        }
        if (Height.Value < 30 || Height.Value >= 250)
        {
            ModelState.AddModelError(nameof(Height), "身長は 30.0〜249.9 cm の範囲で入力してください。");
            return false;
        }
        if (Weight.Value < 0.5 || Weight.Value >= 300)
        {
            ModelState.AddModelError(nameof(Weight), "体重は 0.5〜299.9 kg の範囲で入力してください。");
            return false;
        }

        return true;
    }

    private void RecalcBase( bool addErrors )
    {
        // 初期化
        BmrResult = null;
        BodyIndex = null;
        BodySurfaceArea = null;
        AdjustedWeight = null;
        CorrectedWeight = null;

        BmrWeightBasis = null;
        BmrWeightUsed = null;

        BmrKcal = Kcal25 = Kcal30 = Kcal35 = null;
        BmrFormulaDisplay = "";

        if (!CanCalcBase(addErrors))
            return;

        if (Gender != GenderType.Female || !Age.HasValue || Age.Value < 18 || Age.Value > 55)
            IsPregnant = false;


        // BMR / 体格 / BSA
        BmrResult = BmrCalculator.Calculate(Age!.Value, Weight!.Value, Height!.Value, Gender);
        BmrFormulaDisplay = BmrResult.Formula.ToShortName();
        BmrFormulaDisplayLong = BmrResult.Formula.ToLongName();
        BodyIndex = BodyIndexCalculator.Calculate(Age.Value, Height.Value, Weight.Value, Gender);
        BodySurfaceArea = BodySurfaceAreaCalculator.Calculate(Height.Value, Weight.Value);

       
        // ※ BodyIndex が計算済みである前提
        double obesityDegree = BodyIndex?.ObesityDegree ?? 0;

        // まず basis を決める（nullにしない）
        BmrWeightBasis = AdjustedWeightCalculator.GetBasis(Age.Value, obesityDegree);

        // BodyIndex はこの時点で存在する前提（CanCalcBase通過＋Calculate済み）
        var bodyIndex = BodyIndex!;

        // 調整体重そのもの（式で算出した値）を保持
        AdjustedWeight = AdjustedWeightCalculator.CalculateAdjusted(Weight.Value, bodyIndex.StandardWeight);
                
        // 補正体重（BMR/エネルギー/蛋白などの基礎として使う“最終採用体重”）
        CorrectedWeight = BmrWeightBasis switch
        {
            BmrWeightBasisType.Standard => bodyIndex.StandardWeight,
            BmrWeightBasisType.Adjusted => AdjustedWeight.Value,
            _ => Weight.Value
        };

        // BMRで用いる体重（値）
        BmrWeightUsed = CorrectedWeight.Value;


        // デバッグ用
#if DEBUG
        DebugWeightLine =
         $"ObesityDegree={obesityDegree}  " +
         $"Basis={BmrWeightBasis}  " +
         $"Actual={Weight.Value:0.0}  " +
         $"Std={BodyIndex?.StandardWeight:0.0}  " +
         $"Adj={AdjustedWeight:0.0}  " +
         $"Corr={CorrectedWeight:0.0}";
#else
        DebugWeightLine = "";
#endif  

        // 表示用エネルギー候補（整数）
        BmrKcal = (int)Math.Round(BmrResult.RawValue, MidpointRounding.AwayFromZero);
        

        // 25/30/35 は標準体重ベース（年齢に関係なく表示する方針に寄せる）
        // ※ StandardWeight が計算できている前提
        Kcal25 = (int)Math.Round(bodyIndex.StandardWeight * 25.0, MidpointRounding.AwayFromZero);
        Kcal30 = (int)Math.Round(bodyIndex.StandardWeight * 30.0, MidpointRounding.AwayFromZero);
        Kcal35 = (int)Math.Round(bodyIndex.StandardWeight * 35.0, MidpointRounding.AwayFromZero);
    }
    

    // ==============================
    // エネルギー/蛋白/水分（参考表示）
    // ==============================
    private void RecalcEnergyProteinWater()
    {
        EnergyFinal = null;
        EnergyByBmrKcal = null;
        ProteinRaw = null;
        ProteinDisplayText = null;
        WaterDisplay = null;
        WaterFeverCorrected = false;

        // ストレスは入力が揃わなくても計算可能
        StressBase = StressFactorTable.Get(StressFactor);
        StressTemperature = TemperatureStressTable.GetAddition(SelectedBodyTemperature);
        StressPressureUlcer = PressureUlcerStressTable.GetAddition(SelectedPressureUlcer);
        StressTotal = StressBase + StressTemperature + StressPressureUlcer;

        if (!CanCalcBase(addErrors: false) || BodyIndex is null || CorrectedWeight is null || BmrResult is null)
            return;

        // BMR推定エネルギー
        var bmrForEnergy = BmrCalculator.Calculate(Age!.Value, CorrectedWeight.Value, Height!.Value, Gender);

        var energyByBmr =
            Age.Value == 0
                ? ((BmrResult.RawValue * StressTotal) + (40 * Weight!.Value)) * 1.1
                : bmrForEnergy.RawValue
                    * ActivityFactorTable.Get(ActivityFactor)
                    * StressTotal;
        
        // 参考表示用：基礎エネルギー×係数（整数）
        EnergyByBmrKcal = (int)Math.Round(energyByBmr, MidpointRounding.AwayFromZero);
        
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

        ProteinDisplayText =
            Math.Round(ProteinRaw.Value, 1, MidpointRounding.AwayFromZero).ToString("F1");

        // 水分（既存ロジック踏襲）
        double water =
            WaterCalculator.CalculateTotal(
                Age.Value,
                Weight.Value,
                IsHemodialysis,
                IsPregnant,
                AdjustedWeight!.Value,          // 妊娠で肥満度120%以上なら調整体重
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



