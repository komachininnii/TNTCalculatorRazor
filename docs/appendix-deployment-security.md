# Appendix: Deployment & Security

院内マニュアルリンクを安全に運用するための現行ルールと最小限の設定方法、および障害時の基本的なログ確認方法。

## 1. 目的

- 院内配信時のみ「院内マニュアル」リンクを表示する。
- 院外公開（Azure等）では表示しない。

## 2. 表示条件

- `InternalManual.Enabled == true`
- かつ `InternalManual.Url` が空でない

## 3. 設定方法（最小例）

院内マニュアルリンクを有効にするには、`InternalManual` を環境ごとに設定する。

### appsettings.Production.json の例

```json
{
  "InternalManual": {
    "Enabled": true,
    "Url": "http://example.invalid/internal-manual.pdf"
  }
}
```
### 環境変数の例

```Plain text
InternalManual__Enabled=true
InternalManual__Url=http://example.invalid/internal-manual.pdf
```

## 4. 設定の安全方針

- 機微情報はリポジトリや公開環境に含めない。
- `appsettings.json` / `appsettings.Development.json` には院内専用値を書かない。
- 院内専用値は環境ごとの安全な設定手段で管理する。
- URL は環境によっては機微情報にあたる可能性があるため、取り扱いに注意する。

## 5. 実装上の要点

- `Program.cs` で `InternalManual` セクションをバインドする。
- レイアウト側では `Enabled` と `Url` の両方を満たした場合のみリンク表示。

## 6. 院外環境への発行時チェック

1. 公開対象に機微情報を含むファイルが混入していないことを確認。
2. 公開先で院内リンクが表示されないことを確認。
3. 院内環境でのみリンク表示されることを確認。

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
