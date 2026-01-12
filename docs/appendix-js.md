# Appendix: Client-side JavaScript Design Notes

本ドキュメントは、TNTCalculatorRazor における  
**client-side JavaScript（site.js）の設計方針・移行経緯・意図**を記録するための補足資料である。

将来の改修時に  
「なぜこの構成になっているのか」  
「どこを触ると何が起きるのか」  
を素早く思い出せることを目的とする。

---

## 1. 背景（なぜ整理したか）

当初は以下のような構成だった：

- `Index.cshtml` 内に `<script>...</script>` を多数配置
- `oninput / onkeydown / onblur / onclick` などの inline handler を多用
- IE11 / IEモード対応のための個別スクリプトが点在

この構成では、

- HTMLとJSの責務が混在する
- 修正時に影響範囲が追いにくい
- site.js への集約を進める際に挙動差が出やすい

という課題があった。

---

## 2. 現在の基本方針（結論）

### 2.1 JSは `site.js` に集約する

- `_Layout.cshtml` では **body末尾で1回だけ読み込む**
- `Index.cshtml` などのページ側には **原則 `<script>` を置かない**

```html
<script src="~/js/site.js" asp-append-version="true"></script>
```

---

### 2.2 inline handler は使わず `data-*` で宣言する

HTML側は「**何をしたいか**」だけを書く。

例：

```html
<input type="number"
       data-enter-action="anthro"
       data-smart-blur="1"
       data-maxint="3"
       data-maxdec="1" />
```

JS側は「**どう実行するか**」を一元管理する。

メリット：

- HTML構造変更に強い
- JSの影響範囲が明確
- 将来の入力項目追加が安全

---

## 3. 数値入力制御（tntLimitNumber）

### 3.1 目的

- 数値入力の桁数・小数桁数を物理的に制限
- スマホ / PC / IE11 すべてで同じ挙動を保証

### 3.2 実装方針

- `oninput` は使わない
- `data-maxint / data-maxdec / data-sign` を見て制御
- `document.addEventListener("input", ...)` でイベント委譲

### 3.3 IE11 対応

- `startsWith / endsWith / closest / forEach` 等は使用しない
- ES5 構文のみで実装

---

## 4. Enter / blur / change による再計算制御

### 4.1 Enter送信

- `data-enter-action` が付いた **input要素のみ**対象
- textarea / select / その他要素は除外

```js
var tag = (t.tagName || "").toLowerCase();
if (tag !== "input") return;
```

意図しない Enter 奪取を防ぐための安全策。

---

### 4.2 blur送信（smart blur）

- 入力継続中（別inputへフォーカス移動中）は送信しない
- Weight + Cr → renal を優先
- Weight + Height → anthro

これは **スマホでの入力体験を壊さないための設計**。

---

### 4.3 多重送信防止

- Enter直後の blur による二重 submit を防ぐため
- `tntSkipNextBlurSubmit` フラグを使用
- submit は `submitGuarded()` 経由で統一

---

## 5. `<details>` の設計と IE11 対応

### 5.1 `<details>` の使い分け

- `details.fold`  
  → 折りたたみUIとして使用（IE11 polyfill対象）

- `details.result-details / enteral-details`  
  → PCでは初期open、スマホでは初期close

---

### 5.2 IE11 polyfill の方針

- `<details>` 非対応のため、`summary` クリックを自前実装
- `open` 属性を「状態の唯一の情報源」とする
- **対象は `details.fold` のみに限定**（将来事故防止）

---

### 5.3 スマホでの初期close処理

- スマホ判定時に `open` 属性を削除
- IE polyfill はその状態をそのまま反映

---

## 6. スマホでの「一瞬開いてから閉じる」問題について

### 6.1 原因

- HTMLは `open` 付きで返却される
- 描画後に JS が `open` を外すため、  
  **一度 open 状態が見えてから close される**

これは site.js に移行して **描画とJS実行が分離された結果**であり、
不具合ではなくタイミング差による視覚的現象。

---

### 6.2 対策（採用）

- CSSで「JS初期化完了まで非表示」にする
- JS完了後に `html.tnt-ready` を付与して表示

この方法により、

- ロジックは変更せず
- 端末性能やブラウザ差に依存せず
- 視覚的違和感のみを解消

できる。

---

## 7. ヘルプウィンドウ（公式一覧）

- inline `onclick` は使用しない
- `data-help-window` 属性で宣言
- click イベント委譲で `openHelpWindow()` を呼ぶ

```html
<a href="/Help" data-help-window>公式一覧</a>
```

---

## 8. 今後のメンテナンス指針（重要）

- `site.js` は **1回だけ読み込む**
- 新しい入力挙動は必ず `data-*` で宣言
- IE11 を切る場合：
  - details polyfill
  - ES5縛り
  をまとめて撤去可能

---

## 9. まとめ

- 今回の整理により  
  **HTML / JS / 表示仕様の責務が明確に分離**された
- 挙動は仕様どおり、かつ将来変更しやすい構造
- 本ファイルは「なぜこうなっているか」を残すための記録である
