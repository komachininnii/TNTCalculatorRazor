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
- 対応：pill を **`dd` 側へ移動**

```html
<dd class="dd-inline">
    @if (Model.ActualBmrDisplayKcal.HasValue)
    {
        <strong>@Model.ActualBmrDisplayKcal.Value</strong>
        <span class="unit">kcal</span>
    }
    else
    {
        <span class="muted">-</span>
        <span class="unit">kcal</span>
    }
    <span class="pill mini trunc" title="@Model.ActualBmrFormulaDisplayLong">
         @(!string.IsNullOrWhiteSpace(Model.ActualBmrFormulaDisplay) ? Model.ActualBmrFormulaDisplay : "算出法")
    </span>
</dd>
```

### 2.2 BMR表示方針

- 計算前：`[算出法]`
- 計算後：`[Inf]/[DRI]/[HB]/[Gan]`
- 長い説明は `title` に退避

---

## 3. 体重ロジックの整理

体重ロジックの命名ルールや用語の整理は [用語集：基礎代謝量・体重関連の整理](glossary-bmr-weight-terminology.md) に転記

---

## 4. 肥満度スケールと null 問題（解決済）

~~### 4.1 問題~~

~~- `BmrWeightBasis` が null のまま評価され、常に実測体重に落ちるケースが発生~~

~~### 4.2 対応~~

~~- **順序保証**を明示~~

```csharp
//旧設計での対応例
// 1) basis 決定
CorrectedBmrWeightBasis = AdjustedWeightCalculator.GetBasis(...);

// 2) AdjustedWeight 算出
AdjustedWeight = ...;

// 3) CorrectedWeight 選択
CorrectedWeight = CorrectedBmrWeightBasis.Value switch { ... };
```
※ 現在は CalculateCorrectedWeight() により
計算結果の確定と null 問題は Domain 側で解消されている。
CorrectedBmrWeightBasis は UI 表示（pill）用の派生情報。

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
 @{
    bool showPregnant =
        Model.Gender == GenderType.Female
            && Model.Age.HasValue
            && Model.Age.Value >= 18
            && Model.Age.Value <= 55;
 }
    <label class="inline-check"
        data-pregnant-wrapper
        title="妊娠かつ肥満度120%以上では、水分計算に調整体重を使用します。"
        @(showPregnant ? "" : "style=\"display:none;\"")>
            <input asp-for="IsPregnant"
                type="checkbox"
                data-change-action="anthro"
                data-pregnant-input />
            妊娠
    </label>
```

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
/* IE用：カードの区切り（上線＋間隔） */
.main .card,
.summary .card {
    border-top: 1px solid #d6d6d6;
    margin-top: 8px;
}
    @supports (display: grid) {
    .main .card,
    .summary .card {
        border-top: 0;
        margin-top: 0;
    }
        .main .card:first-of-type {
            border-top: 0;
            margin-top: 0;
        }
    }
```

※ `first-of-type` による例外指定は構造依存が強いため削除。

---

## 8. スマホ横はみ出し対策

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
  margin-top: 0px; /* カード間の隙間 */
}
```
**カード間の margin の経緯**
- 当初の方針 
  - IE11（IEモード）向けにカード間の仕切り（上線）を出すため、IEカード仕切り線として`.main .card, .summary .card` に 
  `border-top: 1px solid #d6d6d6; margin-top: 8px;` を適用していた。
  - モダンブラウザ側は grid や @supports を使って上線や余白を制御し、IE 用のスタイルはフォールバックとして残す方針だった。
- 意図した上書き  
  - 「モダン/スマホでカード上線消去を回避する」という意図で、最終的に site.css の末尾で上書きルールを置く設計にしたが、
    実装の検証の結果、`margin-top` を 0px にして確定した。


### 8.4 1カラム時の列仕切りの残存解除

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

### 8.5 IEでフッターが右にはみ出す問題の対策
```css
    @media all and (-ms-high-contrast: none), (-ms-high-contrast: active) {
        /* 横スクロール封じ込め（IE特有のはみ出し対策） */
        .appbar-footer {
            overflow-x: hidden;
    }
```
