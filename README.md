# TNTCalculatorRazor

## 概要
経腸栄養量（必要エネルギー・蛋白・水分）を算出する Web アプリ。  
ASP.NET Core Razor Pages で実装している。

- 院外公開版：個人 Azure App Service で公開  
- 院内版：院内サーバーから配信（専用PC・HD画角のみ）

---

## 基本設計方針（最重要）
- ソースは **1系統のみ**（院内／院外で分岐しない）
- 院内限定情報（院内マニュアルURL等）は **設定で制御**
- GitHub / Azure に **院内情報を絶対に混入させない**

この方針により、  
ソース分岐による保守事故や、院内／院外での機能乖離を防止する。

---

## UI 設計の要点

### PC
- 1画面完結
- スクリーンショット運用を想定

### スマートフォン（院外）
- 入力〜「必要エネルギー／蛋白／水分（要点）」まで初期画面1画面内
- 体重・Cr は onblur submit により入力取りこぼしを防止
- Validation エラーは ModelState にエラーがある場合のみ描画し、行間揺れを防ぐ

---

## 院内マニュアルリンクの設計

### 目的
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
launchSettings.json に一時的に追加：

```json
"InternalManual__Enabled": "true",
"InternalManual__Url": "http://127.0.0.1/test.pdf"
```
※ 本物の院内URLはローカル確認では記載しない。
---

## トラブルシューティング

### Azure で院内マニュアルリンクが表示されてしまう場合
- /home/site/wwwroot 配下に appsettings.Production.json が残っていないか確認
- Zip Deploy は不要ファイルを自動削除しない点に注意
- README の「Azure 発行時の安全対策」を参照

---

## 今後
院内サーバーは今後PHPへ移行予定だが、当面は.NETを維持していただけるとのこと。


## UI互換対応メモ

IE11互換、モダンブラウザ、スマートフォン表示に関する
詳細な設計判断・調整履歴は以下を参照。
なお、この際、AdjustedWeight/CorrectedWeightの整理をおこなった（3. 体重ロジックの整理（重要）に記載）
- [Appendix: UI / IE / Mobile 対応メモ](docs/appendix-ui-ie-mobile.md)

※ 妊娠チェックの表示は WaterCalculator の年齢区切り（55/56）に合わせ、
　Female かつ 18～55歳に限定した。

---

## 更新履歴
Ver3.0.0-beta.7 2026/01/08 院内公開
Ver3.0.0-beta.8 2026/01/12 院内更新
Ver3.0.0-beta.9 2026/01/12 IEでフッターが右に飛び出すバグフィックス(院内更新差し替え）
