# TNTCalculatorRazor UI調整メモ（IE11互換・モダン・スマホ対応）

本ドキュメントは、TNTCalculatorRazor において実施した **IE11（IEモード）／モダンブラウザ／スマートフォン**の UI 差異調整、および関連するロジック整理を将来振り返れるようにまとめた作業記録です。

---

## 1. 全体方針

- **IE11（IEモード）とモダンブラウザの両立**を最優先
- モダン側（grid/flex/gap）を主設計とし、IE側はフォールバック
- スマホは「補助的利用」を想定し、
  - 横スクロールしない
  - 重要情報は1画面で把握可能
- 計算ロジックは壊さない（UI変更と責務分離を徹底）

---

## 2. BMRまわりの整理

### 2.1 BMR pill の段ズレ対策（IE）

- 問題：
  - `dt` 側に pill を置くと、IE の float/flex フォールバックで行高が不安定
- 対応：
  - pill を **`dd` 側へ移動**
  - ストレス係数と同じ構造に統一

```html
<dd>
  <span class="bmr-value">...</span>
  <div class="pills bmr-pills">...</div>
</dd>
```

### 2.2 BMR表示方針（最終）

- 計算前：
  - `[算出法] [使用体重]`
- 計算後：
  - `[HB] [実測50.0] / [標準50.0] / [調整50.0]`
- 長い説明は `title` に退避

---

## 3. 体重ロジックの整理（重要）

### 3.0 置換ルール（自分ルールとして固定・重要）

今後のコード検索・保守で迷わないため、以下の命名を**固定ルール**として統一する。

- **「標準/実測/調整を切替して使う体重」→ `CorrectedWeight`**
- **「調整体重（式で出した調整値）が必要」→ `AdjustedWeight`**

運用の目安：

- WaterCalculator の第5引数のように **“adjusted（調整体重）” を要求**している箇所は `AdjustedWeight`
- Selector 等で **“補正体重（採用体重）”**として扱う箇所は `CorrectedWeight`

---

## 3. 体重ロジックの整理（重要）

### 3.1 用語整理

- **AdjustedWeight**
  - 調整体重そのもの（式で計算される値）
- **CorrectedWeight**
  - 実測／標準／調整から最終的に選ばれた「補正体重」

### 3.2 プロパティ構成（IndexModel）

```csharp
public double? AdjustedWeight { get; private set; }
public double? CorrectedWeight { get; private set; }

// 表示用エイリアス
public double? BmrWeightFinal => CorrectedWeight;
```

### 3.3 注意点

- `CorrectedWeight` は **BMR / エネルギー / 蛋白**で共通利用
- WaterCalculator では「調整体重そのもの」が必要なため `AdjustedWeight` を使用

---

## 4. 肥満度スケールと null 問題

### 4.1 問題

- `BmrWeightBasis` が null のまま評価され、
  - 常に実測体重に落ちるケースが発生

### 4.2 対応

- **順序保証**を明示

```csharp
// 1) basis 決定
BmrWeightBasis = AdjustedWeightCalculator.GetBasis(...);

// 2) AdjustedWeight 算出
AdjustedWeight = ...;

// 3) CorrectedWeight 選択
CorrectedWeight = BmrWeightBasis.Value switch { ... };
```

---

## 5. 妊娠チェック（Female限定）

### 5.1 仕様

- 性別が Female のときのみ表示
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

- 1300px 以下で縦積み

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

※ `first-of-type` による例外指定は構造依存が強いため最終的に削除

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

---

## 9. 結果

- IE / モダン / スマホで表示差が大幅に縮小
- 横スクロール問題を解消
- UI密度と可読性のバランスを維持
- 計算ロジックへの影響なし

---

## 10. 今後のメモ

- IE対応CSSは将来削除可能なようコメントを残す
- 妊娠チェックは pill 表示などへ拡張可能
- デバッグ欄は運用安定後に非表示化検討

---

（作業記録用 README / MD）

