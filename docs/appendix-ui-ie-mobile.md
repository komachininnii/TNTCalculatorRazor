# TNTCalculatorRazor UI調整メモ（IE11互換・モダン・スマホ対応）

本ドキュメントは、TNTCalculatorRazor において実施した **IE11（IEモード）／モダンブラウザ／スマートフォン**の UI 差異調整と、
関連するロジック整理の背景を記録するための作業メモです。

---

## 1. 全体方針

- **IE11（IEモード）とモダンブラウザの両立**を最優先
- モダン側（grid/flex/gap）を主設計とし、IE側はフォールバック
- スマホは「補助的利用」を想定し、
  - 横スクロールしない
  - 重要情報は 1 画面で把握可能
- 計算ロジックは壊さない（UI 変更と責務分離を徹底）


---

## 2. BMRまわりの整理

### 2.1 BMR pill の段ズレ対策（IE）

- 問題：`dt` 側に pill を置くと、IE の float/flex フォールバックで行高が不安定
- 対応：pill を **`dd` 側へ移動**し、ストレス係数と同じ構造に統一

```html
<dd>
  <span class="bmr-value">...</span>
  <div class="pills bmr-pills">...</div>
</dd>
```

### 2.2 BMR表示方針（最終）

- 計算前：`[算出法] [使用体重]`
- 計算後：`[HB] [実測50.0] / [標準50.0] / [調整50.0]`
- 長い説明は `title` に退避

---

## 3. 体重ロジックの整理

### 3.1 命名ルール（固定）

今後のコード検索・保守で迷わないため、以下の命名を固定ルールとして統一する。

- **「標準/実測/調整を切替して使う体重」→ `CorrectedWeight`**
- **「調整体重（式で出した調整値）が必要」→ `AdjustedWeight`**

運用の目安：

- WaterCalculator の第 5 引数のように **「調整体重」を要求**している箇所は `AdjustedWeight`
- Selector 等で **「補正体重（採用体重）」**として扱う箇所は `CorrectedWeight`

---

### 3.2 用語整理

- **AdjustedWeight**
  - 調整体重そのもの（式で計算される値）
- **CorrectedWeight**
  - 実測／標準／調整から最終的に選ばれた補正体重

### 3.3 プロパティ構成（IndexModel）

```csharp
public double? AdjustedWeight { get; private set; }
public double? CorrectedWeight { get; private set; }

// 表示用エイリアス
public double? BmrWeightFinal => CorrectedWeight;
```

### 3.4 注意点

- `CorrectedWeight` は **BMR / エネルギー / 蛋白**で共通利用
- WaterCalculator では「調整体重そのもの」が必要なため `AdjustedWeight` を使用

---

## 4. 肥満度スケールと null 問題（解決済）

~~### 4.1 問題~~

~~- `BmrWeightBasis` が null のまま評価され、常に実測体重に落ちるケースが発生~~

~~### 4.2 対応~~

~~- **順序保証**を明示~~

```csharp
//旧設計での対応例
// 1) basis 決定
BmrWeightBasis = AdjustedWeightCalculator.GetBasis(...);

// 2) AdjustedWeight 算出
AdjustedWeight = ...;

// 3) CorrectedWeight 選択
CorrectedWeight = BmrWeightBasis.Value switch { ... };
```
※ 現在は CalculateCorrectedWeight() により
計算結果の確定と null 問題は Domain 側で解消されている。
BmrWeightBasis は UI 表示（pill）用の派生情報。

---

## 5. 妊娠チェック（Female限定）

### 5.1 仕様

- 性別が Female かつ 年齢18～55 のときのみ表示
- 水分量計算のみに影響
- 毎回説明を出さず、`title` で補足

### 5.2 IndexModel

```csharp
[BindProperty]
public bool IsPregnant { get; set; } = false;
```

### 5.3 Razor

```cshtml
@if (Model.Gender == GenderType.Female)
{
  <label class="inline-check"
         title="妊娠かつ肥満度120%以上では、水分計算に調整体重を使用します。">
    <input asp-for="IsPregnant" onchange="..." />
    妊娠
  </label>
}
```

### 5.4 注意

- `IsPregnant` の **二重定義（BindProperty重複）** に注意

---

## 6. セクション見出し（section-head）

### 6.1 見出し文字の割れ対策

```css
.card .section-head h3 {
  white-space: nowrap;
  word-break: normal;
}
```

### 6.2 中間幅でのはみ出し対策

1300px 以下で縦積み

```css
@media (max-width: 1300px) {
  .card .section-head {
    flex-direction: column;
    align-items: flex-start;
  }
}
```

---

## 7. IE用カード仕切り線

### 方針

- モダン：仕切りなし
- IE：カード間に薄い上線

```css
.main .card,
.summary .card {
  border-top: 1px solid #d6d6d6;
  margin-top: 8px;
}
```

※ `first-of-type` による例外指定は構造依存が強いため削除。

---

## 8. スマホ横はみ出し対策（最終版）

### 8.1 基本セット（site.css 末尾）

```css
/* === smartphone overflow fix === */
*, *::before, *::after { box-sizing: border-box; }

.field .control,
.section-head .section-meta,
.pills,
.kv dd {
  min-width: 0;
}

.field .control select,
.field .control input,
.field .control textarea {
  max-width: 100%;
}

@media (max-width: 520px) {
  .pills { flex-wrap: wrap; }
  .bmr-pills { flex-wrap: nowrap; }
}
```

### 8.2 sel-wide の最終調整

```css
.field-compact .control select.sel-wide {
  max-width: 100%;
}

@media (min-width: 981px) {
  .field-compact .control select.sel-wide {
    max-width: 20rem; /* 好みで調整 */
  }
}
```
### 8.3 モダン/スマホでもカード上線を出す

IE だけで上線を出していた影響でモダンの上線が消えたため、末尾で上書き。

```css
/* モダン/スマホでもカードの区切り線を出す（末尾で確実に上書き） */
.main .card,
.summary .card {
  border-top: 1px solid #d6d6d6;
  margin-top: 8px;
}
```

### 8.4 1カラム時の列仕切りの残存解除

## 1カラム時にIE用の列仕切り（border/padding）が main に残り左列だけ細く見えるため強制解除

1 カラム時に IE 用の border/padding が main に残り、左列だけ細く見えるため強制解除。

```css
/* 1カラム時：左（main）だけ狭く見えるのは、main側に残る余白/罫線が原因のことが多い */
@media (max-width: 980px) {
  .main {
    border-right: 0 !important;
    padding-right: 0 !important;
    margin-right: 0 !important;
  }

  /* 念のため左右も揃える */
  .main, .summary {
    padding-left: 0 !important;
    margin-left: 0 !important;
  }
}
```

## 8.5 IEでフッターが右にはみ出す問題の対策
```css
    @media all and (-ms-high-contrast: none), (-ms-high-contrast: active) {
        /* 横スクロール封じ込め（IE特有のはみ出し対策） */
        .appbar-footer {
            overflow-x: hidden;
    }
```
