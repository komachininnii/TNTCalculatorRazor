using System.ComponentModel.DataAnnotations;

namespace TNTCalculatorRazor.Domain.Enums;

public enum EnteralFormulaType
{
    // =========================
    // 1.0 kcal / mL
    // =========================
    [Display(Name = "メイバランス 1.0（1.0 kcal/mL）")]
    Meibalance10,

    [Display(Name = "ペプタメン インテンス（1.0 kcal/mL）")]
    PeptamenIntense10,

    [Display(Name = "MEIN（1.0 kcal/mL）")]
    Mein10,

    [Display(Name = "グルセルナ REX（1.0 kcal/mL）")]
    GlucernaRex10,

    [Display(Name = "ラコール NF（1.0 kcal/mL）")]
    RacolNF10,

    [Display(Name = "ラコール NF 半固形（1.0 kcal/mL）")]
    RacolNFSemiSolid10,

    [Display(Name = "エレンタール（1.0 kcal/mL）")]
    Elental10,

    // =========================
    // 1.5 kcal / mL
    // =========================
    [Display(Name = "ペプタメン プレバイオ（1.5 kcal/mL）")]
    PeptamenPrebio15,

    [Display(Name = "ペプタメン AF（1.5 kcal/mL）")]
    PeptamenAF15,

    [Display(Name = "アイソカル サポート（1.5 kcal/mL）")]
    IsocalSupport15,

    [Display(Name = "ラクフィア 1.5（1.5 kcal/mL）")]
    Lacphia15,

    [Display(Name = "PGソフト EJ（1.5 kcal/mL）")]
    PGSoftEJ15,

    [Display(Name = "エンシュア H（1.5 kcal/mL）")]
    EnsureH15,

    // =========================
    // 1.6 kcal / mL
    // =========================
    [Display(Name = "リーナレン MP（1.6 kcal/mL）")]
    RenalenMP16,

    [Display(Name = "イノラス（1.6 kcal/mL）")]
    Inoras16
}
