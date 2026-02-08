# 計算結果の丸め仕様と Final 値の設計

本ドキュメントは、エネルギー・蛋白・水分・経腸栄養に関する
**丸め仕様および計算結果プロパティの設計方針**をまとめたものである。

---

## 基本方針

本アプリでは、計算結果を以下の 3 段階で扱う。

| 区分 | 意味 |
|---|---|
| Raw | 計算式から得られた未丸めの値（double） |
| Final | 仕様として確定した丸め後の値 |
| Display | 画面表示専用（文字列変換のみ） |

丸めは **必ず Final を作る段階で一度だけ行う**。

---

## 丸めルールの集約

丸め仕様は `RoundingRules` に集約する。

```csharp
public static class RoundingRules
{
    // kcal/day：四捨五入
    public static int RoundKcalToInt(double kcal)
        => (int)Math.Round(kcal, MidpointRounding.AwayFromZero);

    // g/day：小数 1 桁
    public static double RoundGram1dp(double gram) =>
        => Math.Round(gram, 1, MidpointRounding.AwayFromZero);

    // mL/day：切り上げ
    public static int CeilMl(double ml) =>
        => (int)Math.Ceiling(ml);

    // 経腸栄養 mL/day：四捨五入
    public static int RoundEnteralMl( double ml )
        => (int)Math.Round(ml, MidpointRounding.AwayFromZero);
}
```

---

## Energy（エネルギー）

- Raw：計算途中の double
- Final：仕様として確定した kcal/day（int）

```csharp
EnergyFinal = RoundingRules.RoundKcalToInt(selectedEnergy);
```

---

## Protein（蛋白）

```csharp
ProteinRaw = ProteinCalculator.Calculate(...);

ProteinFinal =
    RoundingRules.RoundGram1dp(ProteinRaw.Value);
```

表示用は計算を含めず、Final から生成する。

```csharp
public string? ProteinDisplayText =>
    ProteinFinal.HasValue
        ? ProteinFinal.Value.ToString("F1")
        : null;
```

---

## Water（水分）

- Raw：double（mL）
- Final：切り上げた mL/day

```csharp
WaterFinal = RoundingRules.CeilMl(WaterRaw.Value);
```

---

## 経腸栄養（Enteral）

- 入力（kcal / mL）に応じて Raw を計算
- 丸めは Final 相当の値を作る段階でのみ行う
- 表示用 kcal / mL は再計算せず Final を基準とする

---

## BMR 系プロパティの整理

- ActualBmr：実測体重ベース
- CorrectedBmr：体重補正ロジックの内部計算用
- EstimatedEnergyByCorrectedBmr：参考表示用

曖昧だった `BmrKcal` は意味を失ったため削除した。

---

## 設計の根拠

- 丸めの重複・揺れを防ぐ
- 表示変更が計算結果に影響しないようにする
- どこで仕様が確定するかを明確にする
