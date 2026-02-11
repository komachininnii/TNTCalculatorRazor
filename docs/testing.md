# Testing

## 方針
- 計算ロジックの正しさは Domain のユニットテストで担保する（UI/Indexの統合テストは原則余力枠）。
- 仕様の境界（年齢・肥満度・疾患・丸め）を優先的に固定する。

## 実行方法
- Visual Studio: テスト エクスプローラーから実行
- CLI: `dotnet test`

## 主要テスト
- BMR: `BmrCalculatorTests`
- 標準体重: `StandardWeightCalculatorTests`
- 補正体重: `CorrectedWeightCalculatorTests`
- エネルギー/蛋白: `ProteinCalculatorTests`, `ProteinRuleTests`, `WeightForCalculationSelectorTests`
- 水分: `WaterCalculatorTests`
- CCr: `CcrCalculatorTests`, `CcrCreatinineCorrectionRuleTests`, `CcrCalculator_WithCorrection_Tests`
- 丸め: `RoundingRulesTests`
- 統合：`IndexIntegrationTests`

## 補足
- CCrは常に実測体重を使用する。
- Energy/Proteinは原則CorrectedWeight（Proteinは例外疾患で標準体重、小児は実測体重）。
