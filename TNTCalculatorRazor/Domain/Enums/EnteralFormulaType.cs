using System.ComponentModel.DataAnnotations;

namespace TNTCalculatorRazor.Domain.Enums;

public enum EnteralFormulaType
{
    [Display(Name = "メイバランス1.0 [ 300(300) / 400(400) ]")]
    Meibalance10,

    [Display(Name = "ペプタメンプレビオ [ 200(300) / 267(400) ] 消化態")]
    PeptamenPrebio15,

    [Display(Name = "ペプタメンインテンス [ 200(200) ] 侵襲期")]

    PeptamenIntense10,
    [Display(Name = "ペプタメンAF [ 200(300) ] 術後･回復期")]
    PeptamenAF15,

    [Display(Name = "アイソカルサポート [ 200(300) / 267(400) ] 乳糖0")]
    IsocalSupport15,

    [Display(Name = "ラクフィア1.5 [200(300) / 267(400)] 乳酸菌")]
    Lacphia15,

    [Display(Name = "MEIN [ 200(200) ] 高蛋白･免疫調整")]
    Mein10,

    [Display(Name = "リーナレンMP [ 250(400) ] 腎不全")]
    RenalenMP16,

    [Display(Name = "グルセルナREX [ 200(200) / 400(400) ] 糖尿病")]
    GlucernaRex10,

    [Display(Name = "PGソフトEJ [ 200(300) / 267(400) ] 粘度2万,胃瘻")]
    PGSoftEJ15,

    [Display(Name = "ラコールNF [ 200(200) ] 脂質20%,処方")]
    RacolNF10,

    [Display(Name = "ラコール半固形 [ 300(300) ] 胃瘻,処方")]
    RacolNFSemiSolid10,

    [Display(Name = "エンシュアH [ 250(375) ] 脂質32%,処方")]
    EnsureH15,

    [Display(Name = "イノラス [ 187.5(300) ] 長期摂取困難,処方")]
    Inoras16,
   
    [Display(Name = "エレンタール [ 300(300) ] 成分栄養,処方")]
    Elental10,

}
