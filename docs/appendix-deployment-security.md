# appendix-deployment-security.md

## 本書の位置づけと前提

本書は **TNTCalculatorRazor を Visual Studio から Azure App Service へ直接 Publish（Zip Deploy）する運用**を前提としている。

- GitHub Actions は使用していない
- CI/CD は構成していない
- Visual Studio の「発行（Publish）」機能を用いた Zip Deploy を採用している

以下の内容は、**実際に発生した挙動と、それに対して行った対策の記録**であり、  
Azure App Service / Zip Deploy の一般的な仕様を断定するものではない。

---

# 院内マニュアルリンクの設計

## 目的

院内サーバー配信時のみ、  
フッターの「公式一覧」の右に「院内マニュアル」リンクを表示する。

Azure（院外公開）では **一切表示しない**。

---

## 表示制御の考え方

- 設定キー **InternalManual** の内容で表示を制御する
- 院内 URL は **ソースコード・GitHub・Azure 発行物に含めない**
- URL は院内サーバー上の設定ファイルのみで管理する

### 表示条件

- `InternalManual.Enabled == true`
- かつ `InternalManual.Url` が空でない

---

## InternalManual 設定モデル

### 設定項目

- **Enabled** : 院内マニュアルリンクを表示するかどうか
- **Url** : 院内 PDF へのリンク

### appsettings.json（例）

```json
{
  "InternalManual": {
    "Enabled": false,
    "Url": ""
  }
}
```

※ appsettings.json / Development.json には実 URL を記載しない。

---

## 設定ファイルの扱い（安全設計）

### GitHub に含める設定ファイル

- appsettings.json
- appsettings.Development.json

※ いずれにも院内 URL 等の機微情報は含めない。

### GitHub に含めない設定ファイル

- **appsettings.Production.json**

院内専用の URL・設定は、このファイルのみに記載する。

### .gitignore 設定例

```gitignore
# Internal-only settings
appsettings.Production.json
```

---

## Azure Publish（Zip Deploy）で実際に起きたこと

### 実際の経緯

以下は **実際に確認・体験した手順**である。

1. `.gitignore` に appsettings.Production.json を追加
2. csproj に以下を設定

```xml
<ItemGroup>
  <Content Update="appsettings.Production.json">
    <CopyToOutputDirectory>Never</CopyToOutputDirectory>
    <CopyToPublishDirectory>Never</CopyToPublishDirectory>
  </Content>
</ItemGroup>
```

3. appsettings.Production.json を作成し、仮の院内 URL を記載
4. GitHub への push と「フォルダー発行」では **含まれないことを確認**
5. Azure App Service へ Publish（Zip Deploy）

➡ **Azure 側に appsettings.Production.json が配置されてしまった**

---

### 追加で発生した挙動

- Azure SSH で appsettings.Production.json を削除
- 再 Publish を実行

➡ **再び appsettings.Production.json が配置された**

この時点で、

- `.gitignore`
- CopyToPublishDirectory = Never
- CopyToOutputDirectory = Never

はいずれも **Azure Publish には十分でなかった**。

---

## 最終的に有効だった対策

### pubxml による除外指定（補助）

```xml
<PropertyGroup>
  <ExcludeFilesFromDeployment>appsettings.Production.json</ExcludeFilesFromDeployment>
</PropertyGroup>
```

---

### csproj による完全除外（決定打）

```xml
<ItemGroup>
  <!-- Internal-only settings: never publish, never copy -->
  <Content Remove="appsettings.Production.json" />
  <None Remove="appsettings.Production.json" />
</ItemGroup>
```

この設定により：

- Azure Publish に含まれなくなった
- 再発行しても復活しなくなった
- ソリューションエクスプローラーでは非表示になり、
  「すべてのファイルを表示」でのみ確認できる状態になった

---

## 安全対策の整理（実運用）

- appsettings.Production.json は **csproj で Remove する**
- pubxml は保険として併用
- Publish 後、初回のみ Azure 側の残骸を確認

### Azure SSH 確認コマンド

```bash
ls -la /home/site/wwwroot | grep appsettings
```

---

## Program.cs（静的ファイル配信）

Production 環境でもレイアウト差異を出さないため、従来方式を採用。

```csharp
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
```

※ `MapStaticAssets()` / `WithStaticAssets()` は使用しない。

---

## ローカル確認方法

院内リンク表示のみを確認したい場合、  
Development 環境のまま **環境変数で一時的に上書き**する。

### launchSettings.json（例）

```json
"InternalManual__Enabled": "true",
"InternalManual__Url": "http://example.invalid/internal-manual.pdf"
```

※ 実際の院内 URL はローカル環境に記載しない。

---

## トラブルシューティング

### Azure で院内マニュアルリンクが表示されてしまう場合

- `/home/site/wwwroot` に appsettings.Production.json が残っていないか確認
- Visual Studio Publish（Zip Deploy）では、
  **除外方法を誤ると再配置される可能性がある**
