# トラブルシューティング

障害時のログ確認方法。

## Azureでのログの確認
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
## Windows IIS環境でのログの確認
- Windowsイベントビューアー
  - Windowsログ→アプリケーション
  - ソースが「IIS AspNetCore Module V2」のものを探す
