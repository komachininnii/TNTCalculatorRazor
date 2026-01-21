# Appendix: AJAX再計算と結果パネル同期

## 目的
スマホの戻るボタンで「フォーム再送信の確認」が出ないようにするため、  
自動計算（Enter / smart-blur / change）のPOST遷移を抑止し、AJAXで結果欄のみ更新する。

---

## 実装の概要

### 1. 結果欄をPartial化
- 右カラムの結果欄を `_ResultPanel.cshtml` に切り出し、`#resultPanel` へ描画。  
- AJAX再計算時は結果欄だけを返して差し替える。

### 2. Razor Pagesハンドラ
- `OnPostRecalc()` を追加し、計算処理は `RecalcAll()` へ共通化。  
- `OnPostRecalc()` は Partial を返し、履歴にPOSTを残さない。

### 3. クライアントのAJAX化
- `submitWithRecalc()` で `fetch + FormData` により再計算を実行。  
- `fetch` 非対応（IE等）は従来 `submit` にフォールバック。

### 4. 左カラムの同期
- 結果パネル内に JSON を埋め込み、AJAX後に左欄へ反映する。  
  - エラー（年齢/身長/体重/Cr）  
  - 必要エネルギー（算出方法/入力値/候補）  
  - 疾患/妊娠/肝性脳症/蛋白補正などの表示状態  
- IEで `script[type="application/json"]` が読めないケースに備え、  
  `data-*` 属性にも同じJSONを保持してフォールバックする。

---

## UIの細かな調整
- スマホでの詳細パネル（`details`）は1列時は閉じ、2列時は開く。  
  スクロール時のレイアウト変化で勝手に閉じないよう、ブレークポイント変更時のみ再評価。
- 「手動編集」pillは **エネルギー値が存在し、手動編集フラグが立っている場合のみ表示**。
- 必要エネルギー算出方法がBMRベースの場合のみ採用した体重pillを表示

---

## IE対応ポイント
- JSON読み取りは `textContent/innerText/text/innerHTML` と `data-*` のフォールバックを使用。  
- エラー表示は `display` と `class` の両方で制御し、IEで消えない・表示されない問題を防止。

