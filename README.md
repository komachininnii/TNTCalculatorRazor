TNTCalculatorRazor
概要

経腸栄養量（必要エネルギー・蛋白・水分）を算出する Web アプリ。
PC・スマートフォン双方での臨床利用を想定。

院外公開版：個人 Azure App Service で公開

院内版：院内サーバーから配信（専用PC・HD画角のみ）

基本方針

ソースは1系統のみ（院内／院外で分岐しない）

院内限定情報（マニュアルURL等）は 設定で制御

GitHub（Private）にも Azure（院外）にも 院内URLを混入させない

UI設計の要点
PC

1画面完結

スクリーンショット運用を想定

スマートフォン（院外のみ）

入力〜「必要エネルギー／蛋白／水分（要点）」まで 初期画面1画面内

入力取りこぼし防止のため、体重・Cr は onblur submit

行間揺れ防止のため、Validation エラーは ModelState にエラーがある場合のみ描画

院内マニュアルリンクの設計（重要）
目的

院内サーバー配信時のみ
ヘッダー右側（「公式一覧」の右）に「院内マニュアル」リンクを表示する

院外（Azure）では 一切表示しない

実装方針

設定キー InternalManual の有無・内容で表示を制御

URLはソースコード・GitHub・Azure発行物に含めない

設定モデル
"InternalManual": {
  "Enabled": false,
  "Url": ""
}

表示条件（IndexModel）

Enabled == true

かつ Url が空でない

設定ファイルの扱い（安全設計）
GitHub に含める

appsettings.json

appsettings.Development.json

※ 機微情報は記載しない

GitHub に含めない（.gitignore 済）

appsettings.Production.json
→ 院内URL等を 書いてよい唯一の設定ファイル

.gitignore には以下を追加済み：

appsettings.Production.json

Azure 発行（Publish）時の安全対策

Visual Studio から直接 Azure に Zip Deploy するため、
Publish からも appsettings.Production.json を除外している。

csproj 設定
<ItemGroup>
  <Content Update="appsettings.Production.json">
    <CopyToOutputDirectory>Never</CopyToOutputDirectory>
    <CopyToPublishDirectory>Never</CopyToPublishDirectory>
  </Content>
</ItemGroup>


これにより：

ローカル／院内サーバーでは使用可能

Azure 発行物には 絶対に含まれない

Program.cs（静的ファイル配信）

Production 実行時のレイアウト崩れ対策として、
従来方式の静的ファイル配信を使用。

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();


※ MapStaticAssets() / WithStaticAssets() は使用しない

ローカル動作確認方法
院内リンク表示の確認（おすすめ）

ASPNETCORE_ENVIRONMENT=Development のまま

launchSettings.json の environmentVariables に一時的に以下を追加：

"InternalManual__Enabled": "true",
"InternalManual__Url": "http://127.0.0.1/test.pdf"


→ レイアウトを壊さず、リンク表示のみ確認可能

院内サーバー配信時の運用
方法A（推奨）：情シスに環境変数設定を依頼

InternalManual__Enabled=true

InternalManual__Url=http://<院内PDFのURL>

方法B：設定ファイル配置

appsettings.Production.json を

appsettings.json と同じフォルダに配置

Production 環境で自動読込

注意事項

院内URLは 絶対に GitHub / Azure に入れない

Development / Production 切替は表示確認用途のみに使用

本番挙動は設定で制御する

TODO

情報システム課からの PDF アクセス仕様確定

院内サーバーでの設定方式（環境変数 or ファイル）最終決定

ひとこと（未来の自分へ）

この設計にしておけば、
ソースを分けずに、安全に院内専用リンクを出せる。
迷ったら README のこのページを最初に読むこと。