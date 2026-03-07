# Testing

## 方針
- 計算ロジックの正しさは Domain のユニットテストで担保する（UI/Indexの統合テストは原則余力枠）。
- 仕様の境界（年齢・肥満度・疾患・丸め）を優先的に固定する。

## 実行方法
- Visual Studio: テスト エクスプローラーから実行
- CLI: dotnet test

## 主要テスト
### 1. 必要エネルギー
- BMR（基礎代謝）: `BmrCalculatorTests`
- エネルギー算出（指示量/デフォルト）: `EnergyOrderDefaultsTableTests`

### 2. 体重・体格指標
- 標準体重: `StandardWeightCalculatorTests`
- 補正体重: `CorrectedWeightCalculatorTests`
- 計算に使う体重の選択: `WeightForCalculationSelectorTests`
- 体格指標（BMI/肥満度・BSA）: `BodyIndexCalculatorTests`, `BodySurfaceAreaCalculatorTests`

### 3. 必要蛋白量
- 蛋白計算: `ProteinCalculatorTests`
- 疾患・条件による蛋白ルール: `ProteinRuleTests`

### 4. 必要水分量
-水分計算: `WaterCalculatorTests`

### 5. 腎機能（推定CCr）
- CCr計算（基本）: `CcrCalculatorTests`
- Cr補正ルール: `CcrCreatinineCorrectionRuleTests`
- CCr計算（補正込み）: `CcrCalculator_WithCorrection_Tests`

### 6. 経腸栄養
- mL→kcal換算: `EnteralEnergyCalculatorTests`
- 規格割付（候補生成）: `EnteralPackageAllocatorTests`
- 統合データソース整合（成分/規格）: `EnteralFormulaDataTests`

### 7. 共通ルール
- 丸め規則: `RoundingRulesTests`

### 8. 統合テスト（余力枠）
- Index統合: `IndexIntegrationTests`

## 補足
- CCrは常に実測体重を使用する。
- Energy/Proteinは原則CorrectedWeight（Proteinは例外疾患で標準体重、小児は実測体重）。
