# 計算仕様の概要

本ドキュメントは、TNTCalculator における **中核計算仕様の全体像** をまとめたものです。  
個別の実装メモや変更理由ではなく、現時点で **何をどう計算しているか** を把握するための入口として位置づけます。

詳細な実装背景や個別ロジックは、必要に応じて Appendix / Design Notes を参照してください。

---

## 1. 基礎代謝量（BMR）

基礎代謝量（BMR）は、年齢区分に応じて以下の式を使い分ける。

- 乳児
  - 乳児用の簡易推定式を使用
- 小児
  - 小児用の Schofield 式を使用
- 成人
  - 以下のいずれかを使用
    - Harris-Benedict の式
    - Ganpule らの式

補足：

- 採用式の詳細や経緯は、関連する Design Notes / テストを参照
- 成人で利用する式は、アプリ内の条件に応じて切り替える

---

## 2. 必要エネルギー量

必要エネルギー量は、主に以下の方法で算出する。

### 2-1. BMR × 各種係数

- 基本形
  - BMR × 活動係数 × ストレス係数

- 乳児
  - BMR × ストレス係数 + EEA + SDA
    - EEA (energy expenditure of activity)：生体活動・成長に必要なエネルギー
      - 乳児の EEA は 40 kcal/kg
    - SDA (specific dynamic action of food)：食物摂取・代謝に必要なエネルギー
      - SDA = (BMR × ストレス係数 + EEA) × 10%

- 発熱や褥瘡がある場合はストレス係数に加算する

### 2-2. 体重ベースの簡易計算

- 25～35 kcal / 標準体重
- 疾患や状況に応じて使い分ける

### 2-3. るい痩・肥満時の扱い

- 肥満度 80% 以下では、BMR × 係数による必要エネルギー計算において **標準体重** を使用する
- 肥満度 120% 以上では、BMR × 係数による必要エネルギー計算において **調整体重** を使用する
   - 調整体重 = 標準体重 + (実測体重 − 標準体重) × 0.25

補足：

- どの方法を採用するかは、対象や状態に応じて切り替える
- UI 上の見せ方と内部計算ロジックは一致させる

---

## 3. 標準体重

標準体重は年齢区分に応じて以下を用いる。

- 小児
  - 日本小児内分泌学会式
- 成人
  - BMI 22 を基準とする

補足：

- 標準体重は必要エネルギー量や必要蛋白量などの基準の一つとして使用する
- 一部の計算では実測体重・調整体重との使い分けがある

---

## 4. 経腸栄養剤への接続

必要エネルギー量から、経腸栄養剤の投与量や規格候補へ接続する。

### 基本方針

- 必要エネルギー量から投与量（mL）を計算する
- 製剤ごとの規格候補（Packages）に割り付ける
- 複数規格がある場合は、割付候補として優先したい規格を基準に扱う
  - 通常は、**本数を抑えやすい大きい規格** を優先する

補足：

製剤ごとの成分・規格・割付ロジックの詳細は  
`docs/appendix-enteral-nutrition.md` を参照

---

## 5. 必要水分量

必要水分量は、年齢や病態に応じて以下を用いる。

### 5-1. 小児

- Holliday-Segar の式

### 5-2. 成人

- 18～55歳：35 × 体重
- 56～65歳：30 × 体重
- 66歳以上：25 × 体重

### 5-3. 透析患者

- 15 × 体重（ドライウェイト）

### 5-4. 妊娠時の例外

- 妊娠時に、妊娠前肥満度 120% 以上の場合は **調整体重** を使用する

補足：

- 小児と成人ではロジックが異なるため、年齢区分に応じて分岐する
- 妊娠や透析などの例外条件を優先して適用する

---

## 6. 体温補正

必要エネルギー量や必要水分量には、体温に応じた補正を適用する。

### 基本方針

- 平熱を基準とし、発熱時には補正を加える
- UI 上では体温区分として扱い、内部では対応する補正係数を適用する

補足：

- 詳細な補正式や係数は実装側の定義に従う
- 補正の適用範囲は、関連する計算項目ごとに異なる

---

## 7. 蛋白補正

必要蛋白量は、病態や条件に応じて補正する。

### 基本方針

- 標準体重・実測体重・調整体重のいずれを用いるかを状況に応じて切り替える
- 腎機能や透析の有無などで補正方針を分ける

補足：

- 疾患別の個別ルールがあり、例外条件も多いため、詳細は関連するロジック・テストを参照
- 計算結果の表示と内部選択ロジックの整合を重視する

---

## 8. 推定CCr

推定クレアチニンクリアランス（推定CCr）は、以下の式で算出する。

- Cockcroft-Gault の式（実測体重を使用）

### 例外仕様

- 70歳以上で、血清 Cr が
  - 男性：0.8 以下
  - 女性：0.6 以下

  の場合は、筋量減少の影響を考慮して Round up を適用する  
  （結果欄には Round up 前の値も併せて表示する）

補足：

- この Round up は暫定仕様
- 将来的に見直す可能性がある

---

## 9. 体重の使い分け

本アプリでは、計算項目ごとに使用する体重が異なる。

- 実測体重
- 標準体重
- 調整体重

### 基本方針

- BMR、必要エネルギー、必要蛋白、水分量などで使用する体重の種類が異なる
- 肥満、妊娠、透析などの条件では例外的な選択が発生する

補足：

- どの条件でどの体重を使うかは、個別のルールに従う
- 用語の整理は glossary も参照

---

## 10. 単位

本アプリの計算では、以下の単位を使用する。

- エネルギー量：kcal
- 蛋白量：g
- 水分量：mL
- 体重：kg
- 身長：cm
- 年齢：歳
- 体温：℃
- クレアチニン：mg/dL
- 推定CCr：mL/min

経腸栄養剤に関しては以下の単位を使用する。

- 投与量：mL
- 投与カロリー：kcal
- 成分量：
  - g（蛋白質・糖質・脂質・食塩）
  - µg（ビタミンK）
  - mL（水分量）

その他の単位は、必要に応じて定義する。

---

## 11. 関連ドキュメント

- 経腸栄養剤の詳細  
  [docs/appendix-enteral-nutrition.md](./appendix-enteral-nutrition.md)
- 丸めと最終表示値  
  [docs/calculation-rounding-and-final-values.md](./calculation-rounding-and-final-values.md)
- BMR / 体重用語  
  [docs/glossary-bmr-weight-terminology.md](./glossary-bmr-weight-terminology.md)
- テスト方針  
  [docs/testing.md](./testing.md)
- UI や実装判断  
  [docs/ui-decisions.md](./ui-decisions.md)

--- 

## 12. 参考資料

1. Long CL, Schaffel N, et al. Metabolic response to injury and illness: estimation of energy and protein needs from indirect calorimetry and nitrogen balance.
*JPEN*  3(6): 452-456, 1979
2. Harris JA, Benedict FG. A biometric study of basal metabolism in man. *Proc Natl Acad Sci* 4(12): 370-373, 1918
3. Ganpule AA, Tanaka S, et al. Interindividual variability in sleeping metabolic rate in Japanese subjects. *Eur J Clin Nutr* 61(11): 1256-1261, 2007
4. Schofield WN. Predicting basal metabolic rate, new standards and review of previous work. *Hum Nutr Clin Nutr* 39(Suppl 1): 5-41, 1985
5. 乳児BMR簡易計算式 KPUM小児ICUマニュアル 改訂第7版
6. 日本人の食事摂取基準（2025年版） 厚生労働省
7. 山田陽介. 推定エネルギー必要量とは何かを考察する. 臨床栄養 144(7): 1068-1076, 2024
8. 日本小児内分泌学会 性別･身長別標準体重 https://jspe.umin.jp/medical/taikaku.html
9. DuBois D, DuBois EF. A formula to estimate the approximate surface area if height and weight be known. *Arch Intern Med* 17(6_2): 863-871, 1916
10. Holliday MA, Segar WE. The maintenance need for water in parenteral fluid therapy. *Pediatrics* 19(5): 823-832, 1957
11. Cockcroft DW, Gault MH. Prediction of creatinine clearance from serum creatinine. *Nephron* 16(1): 31-41, 1976

---

## 位置づけ

この文書は、TNTCalculator の中核計算仕様の概要をまとめたものです。  
詳細な変更理由や実装経緯は Design Notes / note 側に分け、ここでは現時点の仕様の全体像を俯瞰できることを優先しています。
