# Appendix: JS / Form Refactor & Debug Handling Summary

## 概要
本ドキュメントは **TNTCalculatorRazor** において実施した JavaScript / Razor Pages の整理・リファクタリング内容をまとめた付録です。
挙動を一切変えずに、可読性・保守性・将来拡張性（複数フォーム対応）を高めることを目的としました。

---

## 対象

- `wwwroot/js/site.js`
- `Index.cshtml` / `Index.cshtml.cs`
- `_Layout.cshtml`

---


## 1. JavaScript 構造整理

### 1.1 二重読み込みの解消
- `_Layout.cshtml` における `site.js` の二重読み込みを解消
- 未使用の jQuery ライブラリを完全削除

### 1.2 inline JavaScript の撤廃
- Razor ページ内の `onchange="...submit()"` を全廃
- HTML には **動作意図のみ** を `data-*` 属性で記述
- 実処理はすべて `site.js` に集約

### 使用する data 属性
| 属性 | 役割 |
|----|----|
| `data-change-action` | 値変更時に設定する Action |
| `data-enter-action` | Enter キー送信時の Action |
| `data-smart-blur` | smart blur 対象 |
| `data-blur-action` | blur 時の固定 Action |

---

## 2. Action 設定ロジックの改善

### 2.1 setAction() の廃止
- グローバル `document.getElementById("Action")` 依存を排除
- すべて **form スコープ内**で `input[name="Action"]` を取得

```js
var actionField = form.querySelector('input[name="Action"]');
```

### 2.2 蛋白補正の特殊処理
- `SelectedProteinCorrection` 変更時に
  - `IsProteinCorrectionUserEdited` を JS 側で更新
- Razor 側にロジックを残さない設計

---

## 3. 複数フォーム対応の伏線

### 3.1 form 属性
```html
<form asp-action="Index" method="post" data-tnt-form="main">
```

### 3.2 getForm() の探索順
```js
function getForm(target) {
    return (target && target.form)
        || document.querySelector("form[data-tnt-form]")
        || document.querySelector("form");
}
```

- 現状は 1 フォームでも安全
- 将来複数フォームになっても JS 変更不要

---

## 4. Debug 表示の安全な扱い

### 4.1 Razor (.cshtml)
```cshtml
@{
#if DEBUG
}
<div class="card">
  <details class="fold">
    <summary>デバッグ（開発中）</summary>
    <pre>@Model.DebugWeightLine</pre>
  </details>
</div>
@{
#endif
}
```

- Debug ビルドのみ HTML を生成
- Release ビルドでは **完全に非存在**

### 4.2 Code-behind (.cshtml.cs)
```csharp
#if DEBUG
DebugWeightLine = "...";
#else
DebugWeightLine = "";
#endif
```

- プロパティは常に存在
- Release でのコンパイルエラーを防止

---

## 5. 得られた効果

- 挙動不変のままコード量と重複を削減
- HTML / JS の責務分離が明確化
- 将来拡張（フォーム分割・UI追加）に耐える構造
- デバッグコードの安全な温存

---

## 6. 今後の指針（参考）
- 新規入力項目は `data-change-action` を付与するだけ
- inline JS は原則禁止
- Debug 表示は `#if DEBUG` を基本とする

