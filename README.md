# TNTCalculatorRazor

## 概要
TNTCalculatorRazor は、必要項目を入力するだけで、必要エネルギーや推定CCr、経腸栄養剤の
投与量・成分が自動で表示される計算支援アプリケーションです。
PC・スマートフォンの両方で利用でき、用途に応じた情報密度と操作性を重視して設計されています。

---

## Features
- 必要エネルギー量の算出
  - 基礎代謝量（BMR）× 活動係数 × ストレス係数
  - 25kcal/標準体重
  - 30kcal/標準体重
  - 35kcal/標準体重
- 推定クレアチニンクリアランス（CCr）の計算
- 標準体重・調整体重を考慮した計算ロジック
- 経腸栄養剤の投与量・成分量の算出
- PC（IE11・モダンブラウザ） / スマートフォンに対応したレスポンシブUI

---

## UI Design Concept

本アプリは、使用環境に応じて表示情報と役割が自然に切り替わることを意図しています。

### PCブラウザ
- 二列表示
- 必要エネルギー計算
- 推定 CCr 計算
- 経腸栄養剤の詳細計算
- 全ての情報を一画面に表示

  → 計画立案・経腸栄養剤検討時の利用を想定

### スマートフォン
- 一列表示
- 必要エネルギー計算
- 推定 CCr 計算
- 詳細な計算結果と経腸栄養剤計算は折りたたみ

  → ベッドサイドでの迅速な確認を想定

---

## Calculation Policy
- BMR は実測体重を用いた推定値を表示
- 必要エネルギー計算では、
  標準体重／調整体重を含めた 「最終採用体重」 を内部で選択
- 計算に使用された体重や算出法は UI 上で明示

---

## Design Principles
- 単一コードベース
- 環境に応じた振る舞い
- 機密情報はコードに含めない
- UIと計算ロジックの分離

---

## Technology Stack
- ASP.NET Core (Razor Pages)
- C#
- JavaScript (minimal, framework-free)：IE11互換
- CSS (IE11 / modern browsers fallback-aware)

---

## Browser Support
- Modern browsers (Chrome, Edge, Firefox)
- Edge IE mode / IE11 (fallback layout)

※ モダン CSS 機能は IE11 ではフォールバック動作となります。

※ IE11 では一部レイアウト・表現が簡略化されますが、計算結果の正確性は保持されます。

---

## Notes
- 本リポジトリには、環境固有の設定値や機密情報は含まれません。
- 実運用に関する詳細な設定・運用ノウハウは別ドキュメントに管理されています。

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

## 設計・仕様メモ（Design Notes）
本アプリのUIおよび計算仕様は、誤操作防止・臨床的安全性・将来の拡張性を重視して設計している。  
設計上の判断理由やUI仕様変更の背景は以下にまとめている。

- [UI / 設計判断メモ](docs/ui-decisions.md)

---

## Appendix
### デプロイ・セキュリティ

院内限定情報をソースに混入させないための設計・運用上の注意点まとめ。
- [Appendix: Deployment / Security Notes](docs/appendix-deployment-security.md)

### UI互換対応メモ

IE11互換、モダンブラウザ、スマートフォン表示に関する詳細な設計判断・調整履歴は以下を参照。
なお、この際、AdjustedWeight/CorrectedWeightの整理をおこなった（3. 体重ロジックの整理に記載）
- [Appendix: UI / IE / Mobile 対応メモ](docs/appendix-ui-ie-mobile.md)

### JS Design Notes

Index内の script を site.js に移行経緯・意図を記録する補足資料。
- [Appendix: Client-side JavaScript Design Notes](docs/appendix-js.md)

- ### JS / Form Refactor & Debug Handling Summary

JavaScript / Razor Pages の整理・リファクタリング内容のまとめ。
挙動を一切変えずに、可読性・保守性・将来拡張性（複数フォーム対応）を高める。
- [Appendix: JS / Form Refactor & Debug Handling Summary](docs/appendix-js-form-refactor.md)

### AJAX再計算と結果パネル同期

スマホの戻るボタンで「フォーム再送信の確認」を出さないようにするための実装。
- [AJAX再計算と結果パネル同期](docs/appendix-ajax-recalc.md)

---

## 改変履歴
詳細な改変履歴は [CHANGELOG.md](./CHANGELOG.md) を参照してください。



