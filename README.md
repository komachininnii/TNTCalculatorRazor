# TNTCalculatorRazor

## 概要
TNTCalculatorRazor は、臨床現場での使用を想定した 栄養評価・必要エネルギー計算支援アプリです。
PC・スマートフォンの両方で利用でき、用途に応じた情報密度と操作性を重視して設計されています。

---

## Features
- 必要エネルギー量の算出
 - 基礎代謝量（BMR）× 活動係数 × ストレス係数
 - 25kcal/標準体重
 - 30kcal/調整体重
 - 35kcal/調整体重
- 推定クレアチニンクリアランス（CCr）の計算
- 標準体重・調整体重を考慮した計算ロジック
- 経腸栄養剤の投与量・成分量の算出（PC表示で特に有用）
- PC / スマートフォン双方に対応したレスポンシブUI

---

## UI Design Concept

本アプリは 使用環境に応じて役割が自然に変わる ことを意図しています。

### Mobile (Smartphone)
- 一列表示
- 必要エネルギー計算
- 推定 CCr 計算
  → ベッドサイドでの迅速な確認を想定

### Desktop (PC)
- 二列表示
- 必要エネルギー計算
- 経腸栄養剤の詳細計算
- 候補値の比較・調整
  → 計画立案・経腸栄養剤検討時の利用を想定

---

## Calculation Policy
- BMR は実測体重を用いた推定値を表示
- 必要エネルギー計算では、
  標準体重／調整体重を含めた 「最終採用体重」 を内部で選択
- 計算に使用された体重や算出法は UI 上で明示

これにより、
- 数値の意味が不明確にならない
- 臨床的な判断と乖離しない
ことを重視しています。

---

## Design Principles
- Single codebase
- No environment-specific logic in source code
- Configuration-driven behavior
- Sensitive or environment-dependent information is never hard-coded
- UI and calculation logic are clearly separated

本プロジェクトは 「コードが置かれる環境を前提にしない」 ことを基本方針としています。

---

## Technology Stack
- ASP.NET Core (Razor Pages)
- C#
- JavaScript (minimal, framework-free)
- CSS (IE11 / modern browsers fallback-aware)

---

## Browser Support
- Modern browsers (Chrome, Edge, Firefox)
- Edge IE mode / IE11 (fallback layout)
※ 一部の最新 CSS 機能は IE11 ではフォールバック動作となります。

---

## Notes
- 本リポジトリには、環境固有の設定値や機密情報は含まれません。
- 実運用に関する詳細な設定・運用ノウハウは別ドキュメントとして管理される想定です。

## License
（※ OSS 化時に追記）

---

## Author
tyama

---

## トラブルシューティング
エラー発生時のログ確認方法。

### Azureでのログの確認
Azure Portal→高度なツール→BashまたはSSH

- ログファイル一覧
   ```bash
  ls -lh /home/LogFiles/*docker.log
  ```

- ログを監視する：終了はCTRL+C
  ```bash
  tail -f /home/LogFiles/*docker.log
  ```
- エラーだけ拾う
  ```bash
  grep -i error /home/LogFiles/*docker.log
  ```
### Windows IIS環境でのログの確認
- Windowsイベントビューアー
  - Windowsログ→アプリケーション
  - ソースが「IIS AspNetCore Module V2」のものを探す

---

## Deployment / Security Notes
院内限定情報をソースに混入させないための
設計・運用上の注意点まとめ。
- [Appendix: Deployment / Security Notes](docs/appendix-deployment-security.md)

## UI互換対応メモ

IE11互換、モダンブラウザ、スマートフォン表示に関する
詳細な設計判断・調整履歴は以下を参照。
なお、この際、AdjustedWeight/CorrectedWeightの整理をおこなった（3. 体重ロジックの整理（重要）に記載）
- [Appendix: UI / IE / Mobile 対応メモ](docs/appendix-ui-ie-mobile.md)

 
## JS / Form Refactor & Debug Handling Summary

JavaScript / Razor Pages の整理・リファクタリング内容のまとめ。
挙動を一切変えずに、可読性・保守性・将来拡張性（複数フォーム対応）を高める。
- [Appendix: JS / Form Refactor & Debug Handling Summary](docs/appendix-js-form-refactor.md)


## JS Design Notes

Index内の script を site.js に移行経緯・意図を記録する補足資料。
- [Appendix: Client-side JavaScript Design Notes](docs/appendix-js.md)


## AJAX再計算と結果パネル同期

スマホの戻るボタンで「フォーム再送信の確認」を出さないようにするために実装。
- [AJAX再計算と結果パネル同期](docs/appendix-ajax-recalc.md)

---

## 改変履歴
- Ver3.0.0-beta.7 2026/01/08 初版
- Ver3.0.0-beta.8 2026/01/12 ストレス合計をなくして結果を見やすく。IEでもブラウザを狭くするとスマホモードに
- Ver3.0.0-beta.9 2026/01/12 IEでフッターが右に飛び出すバグフィックス
- Ver3.0.0 2026/01/15 Priacy Policyを別窓で開くよう修正 ※Helpがスマホで別窓にならないデグレあり
- Ver3.0.3 2026/01/21 Help別窓化修正。算出法セレクトを体重補正代謝量×係数に変更
　　　　　　　　　　　採用体重pillを左列に移動。IE左列52.5→48%へ
