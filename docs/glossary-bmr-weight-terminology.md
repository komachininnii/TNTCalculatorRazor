# 用語・命名ルール整理（Glossary / Terminology）

本ドキュメントは **TNTCalculatorRazor** における  
計算ロジック・UI・テストを横断して用いられる **用語と命名規則の辞書** である。

- 実装詳細（丸め・UI制御など）から切り離し、概念の整理を目的とする  
- 他の appendix / 設計文書からは本ドキュメントを参照する

---

## 1. 体重関連の用語整理

本アプリでは、体重を以下の4系統で明確に区別する。

### ActualWeight（実測体重）
- 実際に測定された体重
- 入力値そのもの
- BMR（基礎代謝量）の算出に用いる

### StandardWeight（標準体重）
- 身長・年齢・性別に基づく理論体重
- 肥満度の算出に用いる
  - 肥満度 = 実測体重 / 標準体重 × 100%
- kcal/kg（25/30/35）計算の基準に用いる
- 肥満度80%以下の必要エネルギー・蛋白計算に用いる

### AdjustedWeight（調整体重）
- 実測体重が標準体重に対して過剰な場合の調整体重
  - 調整体重 = （実測体重 - 標準体重）× 0.25 + 標準体重
- 肥満度120%以上の必要エネルギー・蛋白計算に用いる
- 肥満度120%以上の妊婦の水分計算に用いる
  
### CorrectedWeight（補正体重）
- Actual / Standard / Adjusted の中から**最終的に採用された体重**
- 必要エネルギー・蛋白計算の共通基盤

※乳児では肥満度を算出せず常に実測体重を用いる
 
```csharp
public double? AdjustedWeight { get; private set; }
public double? CorrectedWeight { get; private set; }
```

---

## 2. BMR / 補正代謝量 系の用語整理

### 基本方針
- **BMR（基礎代謝量）** は「実測体重ベース」を指す
- **必要エネルギー計算**は「補正体重ベースの BMR × 係数」で行う
- 両者を明示的に分離することで混乱を防ぐ

---

### 2.1 実測体重ベース（基礎代謝量）

| 名称 | 意味 |
|----|----|
| `ActualBmrRaw` | 実測体重ベースの BMR（double） |
| `ActualBmrDisplayKcal` | 表示用（表示丸め, int） |

```csharp
public double? ActualBmrRaw { get; private set; }

public int? ActualBmrDisplayKcal =>
    ActualBmrRaw.HasValue
        ? RoundingRules.RoundKcalToInt(ActualBmrRaw.Value)
        : null;
```

**補足**  
- 「基礎代謝量」という日本語表記は **ActualBMR のみ**に用いる  
- 実測体重ベースの BMR は常に保持する

---

### 2.2 補正体重ベース（必要エネルギー計算用）

| 名称 | 意味 |
|----|----|
| `correctedBmrRaw` | 補正体重ベースの BMR（ローカル変数） |
| `correctedBmrEnergyRawKcal` | BMR × 係数後のエネルギー（double） |
| `CorrectedBmrEnergyDisplayKcal` | 表示用（表示丸め, int） |

```csharp
var correctedBmrRaw =
    BmrCalculator.Calculate(Age!.Value, CorrectedWeight.Value, Height!.Value, Gender)
        .RawValue;

var correctedBmrEnergyRawKcal =
    Age.Value == 0
        ? ((correctedBmrRaw * StressTotal) + (40 * Weight!.Value)) * 1.1
        : correctedBmrRaw
            * ActivityFactorTable.Get(ActivityFactor)
            * StressTotal;

CorrectedBmrEnergyDisplayKcal =
    RoundingRules.RoundKcalToInt(correctedBmrEnergyRawKcal);
```

**補足**
- `CorrectedBmrRaw` は中間値であり、**プロパティ化しない**
- UI に表示されるのはエネルギー換算後のみ

---

## 3. Raw / Final / Display の命名ルール

| 接尾語 | 意味 |
|----|----|
| `Raw` | 内部計算用の未丸め値 |
| `Final` | 仕様として確定した値（仕様丸め） |
| `Display` | 表示専用（UI用） |

**原則**
- 仕様丸め（臨床判断に使う値）は Domain（Rules）で行う
- Display は参照専用とし、再計算しない

---

## 4. EnergyOrderType と命名の考え方

### BMR 系算出方法の命名

**補正体重ベースであることを明示**するため、
従来の `BmrEstimated` は`CorrectedBmrBased`に変更。

```csharp
public enum EnergyOrderType
{
    [Display(Name = "体重補正代謝量×係数")]
    CorrectedBmrBased,

    [Display(Name = "25kcal/標準体重")]
    Kcal25,

    [Display(Name = "30kcal/標準体重")]
    Kcal30,

    [Display(Name = "35kcal/標準体重")]
    Kcal35,

    [Display(Name = "手入力")]
    Manual
}
```

---

## 5. UI 表示用の補助情報

### 補正体重の表示（pill）

- 必要エネルギー算出が `CorrectedBmrBased` の場合のみ表示
- 「どの体重が採用されたか」を明示する

```csharp
public BmrWeightBasisType? CorrectedBmrWeightBasis { get; private set; }
public double? CorrectedBmrWeightUsed { get; private set; }
```

---

## 6. 注意点まとめ

- **基礎代謝量 = ActualBMR**
- **必要エネルギー計算 = CorrectedBMR × 係数**
- `CorrectedWeight` は エネルギー / 蛋白で共通利用
- WaterCalculator は `AdjustedWeight` を直接使用する場合がある
