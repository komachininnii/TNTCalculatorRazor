# 院内マニュアルリンクの設計

## 目的
院内サーバー配信時のみ、  
ヘッダー右側（「公式一覧」の右）に「院内マニュアル」リンクを表示する。

院外（Azure）では **一切表示しない**。

### 表示制御の考え方
- 設定キー **InternalManual** の内容で表示を制御
- URL はソースコード・GitHub・Azure 発行物に含めない

### 表示条件
- InternalManual.Enabled が true  
- かつ InternalManual.Url が空でない

---

## InternalManual 設定の考え方

### 設定モデル（概念）
InternalManual には以下の2項目を持たせる。

- Enabled：院内マニュアルリンクを表示するかどうか  
- Url：院内PDFへのリンク

**設定例（appsettings.json）**
```json
{
  "InternalManual": {
    "Enabled": false,
    "Url": ""
  }
}
```

---

## 設定ファイルの扱い（安全設計）

### GitHub に含める設定ファイル
- appsettings.json  
- appsettings.Development.json  

※ いずれにも院内URL等の機微情報は記載しない。

### GitHub に含めない設定ファイル
- appsettings.Production.json  

院内専用のURLや設定は、このファイルにのみ記載する。

**.gitignore 例**
```
# Internal-only settings
appsettings.Production.json
```
---

## Azure 発行（Publish）時の安全対策

### Zip Deploy の重要な挙動
Azure App Service（Zip Deploy）は、

- 発行物に含まれないファイルを  
- 自動的には削除しない  

という挙動を持つ。

そのため、過去の発行で一度でも  
appsettings.Production.json が配置されていると、  
以後の発行で除外しても Azure 側に残存する可能性がある。

**残骸確認（Azure /home/site/wwwroot）**
1. Azure Portal → App Service → SSH
2. 確認コマンド
   ```bash
   ls -la /home/site/wwwroot | grep appsettings
   ```
3. 見つかった場合は削除後、アプリを再起動
   ```bash
   rm /home/site/wwwroot/appsettings.Production.json
   ```

---


### 安全対策の基本方針
- appsettings.Production.json は **Publish 対象から完全に除外**
- csproj 側で根本的に Publish 入力から外す
- pubxml 側でも除外指定を行う（保険）

---

### csproj による除外設定（確定版）
appsettings.Production.json を  
Publish の入力段階から完全に外す。

**csproj 設定例**
```xml
<ItemGroup>
  <!-- Internal-only settings: never publish, never copy -->
  <Content Remove="appsettings.Production.json" />
  <None Remove="appsettings.Production.json" />
</ItemGroup>
```

この設定により、

- GitHub に含まれない  
- Azure Publish に含まれない  
- Zip Deploy による復活事故を防止できる  
- ソリューションエクスプローラーでは非表示になり「すべてのファイルを表示」で見えるようになる
---

### pubxml による除外設定（保険）
Zip Deploy の発行プロファイル（.pubxml）に、  
appsettings.Production.json を除外する指定を追加する。

**pubxml 設定例**
```xml
<PropertyGroup>
  <!-- Never deploy internal-only settings -->
  <ExcludeFilesFromDeployment>appsettings.Production.json</ExcludeFilesFromDeployment>

  <!-- Clean up extra files on server -->
  <SkipExtraFilesOnServer>false</SkipExtraFilesOnServer>
</PropertyGroup>
```

## Program.cs（静的ファイル配信）

Production 環境でもレイアウトが崩れないよう、従来方式で静的ファイル配信を行う。

```csharp
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
```

※ MapStaticAssets() / WithStaticAssets() は使用しない。
---

### Azure 側の初期確認
初回のみ、Azure 側に設定ファイルの残骸が残っていないか確認する。

確認対象：
- /home/site/wwwroot 配下

**Azure SSH での確認方法**
```bash
ls -la /home/site/wwwroot | grep appsettings
```
## ローカル動作確認方法
院内リンク表示だけ確認したい場合は、Development のまま環境変数で上書きする（見た目を壊さない）。
launchSettings.json に一時的に追加（https側の"ASPNETCORE_ENVIRONMENT": "Development"の下）：

```json
"InternalManual__Enabled": "true",
"InternalManual__Url": "http://example.invalid/internal-manual.pdf"
```
※ 本物の院内URLはローカル確認では記載しない。
---

## トラブルシューティング

---

### Azure で院内マニュアルリンクが表示されてしまう場合
- /home/site/wwwroot 配下に appsettings.Production.json が残っていないか確認
- Zip Deploy は不要ファイルを自動削除しない点に注意

