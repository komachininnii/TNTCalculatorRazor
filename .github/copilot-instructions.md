# Copilot Instructions

## 一般

- Visual Studio でファーストチャンス例外が表示されてもアプリの動作に影響しない場合は無視してよい。Visual Studio IDE 固有の挙動である。

---

## コーディングスタイル

### Enum ベースの static メソッドパターン

係数テーブルおよび単純な `enum` ルックアップは、`static` クラスの単一メソッドとして実装する。  
複合データ（例: EnteralFormulaTable）の参照はこの限りでなく、辞書 + 補助メソッド構成を許容する。  
ロジックは原則 `switch` 式だが、複数条件（年齢＋疾患等）が必要な場合は `if` との組み合わせも許容する。  
積算係数（`Get`）では未定義の enum 値に `ArgumentOutOfRangeException` をスローする。  
加算係数（`GetAddition`）・デフォルト選択（`GetDefault`）の扱いは下表を参照。

```csharp
public static class SomeFactorTable
{
    public static double Get(SomeFactorType type) =>
        type switch
        {
            SomeFactorType.Foo => 1.2,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "未定義の係数です。")
        };
}
```

### テーブルのメソッド名と未定義値の扱い

係数の種類によってメソッド名と未定義値の扱いが異なる。

| 種別 | メソッド名 | 未定義値の扱い |
|---|---|---|
| 積算係数（multiplier） | `Get(EnumType)` | `ArgumentOutOfRangeException` をスロー |
| 加算係数（additive） | `GetAddition(EnumType)` | `0.0` を返す（安全側フォールバック） |
| デフォルト選択（selector） | `GetDefault(...)` | 安全な enum 値を返す（引数は文脈に応じて複数可）

加算係数テーブルで未定義値が `0.0` になるのは意図した設計であり、
「加算しない＝影響なし」が安全側のため例外ではなく `0.0` で返す。

### 係数のデフォルト値

- **積算係数（multiplier factor）** のデフォルト値は `1.0`
- **加算係数（additive factor）** のデフォルト値は `0.0`

---

## IE11 互換性

IE11 サポートが必須。CSS 新機能（CSS Grid の一部、`gap` 等）は `@supports` ガードで包み、IE 側フォールバックを先に記述する。  

```css
/* IE フォールバック — flex で横並び、狭い画面は flex-direction: column で縦積み */
.layout {
    display: flex;
    align-items: flex-start;
}
@media (max-width: 980px) {
    .layout { flex-direction: column; }
}
@supports (display: grid) {
    .layout {
        display: grid;
        grid-template-columns: minmax(0, 1fr) minmax(0, 1fr);
        gap: 12px;
    }
}
```

- `details` / `summary` の開閉状態は `open` 属性のみに依存しない（IE 系で同期ズレが起きる）。
- JS の mobile 判定ブレークポイントは 980px を基準とする。
- JS は 980px、モダン CSS は表示崩れ回避で 1024px の場合あり。

---

## Razor / AJAX 実装上の注意

### AJAX 再計算の契約

本画面は通常 POST に加えて `OnPostRecalc()` を持ち、AJAX 再計算では `handler=Recalc` に POST して `_ResultPanel` の HTML のみを返す。  
`site.js` は `#resultPanel` を差し替えた後、結果パネル内の JSON を使って左カラムのエラー表示・必要エネルギー欄・条件付き UI を再同期する。  
そのため、`_ResultPanel` を変更する際は、HTML 本体だけでなく `resultPanelErrorData` / `resultPanelEnergyData` / `resultPanelFormStateData` および `#resultPanelData` の `data-*` フォールバックも一体として扱うこと。  
AJAX 再計算で返す partial 名、`id` 名、JSON キー名を不用意に変更しない。

### 入力イベントは data-* 属性駆動で実装する

入力欄ごとの送信契機や数値制限は、inline JavaScript ではなく `data-*` 属性で宣言し、`site.js` 側のイベント委譲で処理する。  
主な契約は次のとおり。

- 数値制限: `data-maxint` / `data-maxdec` / `data-sign`
- Enter 送信: `data-enter-action`
- change 送信: `data-change-action`
- blur 送信: `data-blur-action`
- 自動 blur 判定: `data-smart-blur`

新規 input / select / checkbox を追加する場合も、このパターンに合わせること。  
個別要素への inline handler やページ固有の都度バインドを増やさない。

### 手動編集フラグと同期方向

`IsEnergyUserEdited`、`IsEnteralVolumeUserEdited`、`IsProteinCorrectionUserEdited` は hidden で保持し、サーバー・クライアント間で同期する。  
これらは UI 表示用ではなく、**自動追従を止めるための状態**でもあるため、関連ロジックを変更する際は必ず整合性を保つこと。

特に次を守る。

- `energy` 操作時は `IsEnergyUserEdited = true`
- `volume` 操作時は `IsEnteralVolumeUserEdited = true`
- `energy` 操作時は volume 側手動編集フラグを落とす
- `protein` 操作時のみ蛋白補正の手動編集フラグを更新する
- `SelectedProteinCorrection == None` に戻した場合は、自動追従へ復帰させる

必要エネルギーと経腸投与量は**完全な双方向自動同期ではない**。  
energy 側を基準に mL を再計算する流れを優先し、以前の双方向自動反映に戻さないこと。

### 経腸栄養は「表示投与量」を正とする

経腸栄養の kcal→mL 変換では、まず raw volume を計算し、その後 `RoundingRules.RoundEnteralMl(...)` で仕様丸めした `EnteralVolume` を確定値とする。  
`EnteralEnergy`、割付候補、蛋白・脂質・糖質・食塩・ビタミンK・水分量などの成分表示は、常にこの**表示投与量ベース**で再計算する。  
raw volume や丸め前 kcal を表示値として直接使わない。

また、`volume` 操作時に mL から kcal は再計算するが、kcal 入力欄へ自動逆書き戻しはしない。

### 内部リンクは asp-page 優先

アプリ内ページへのリンクは、`href="/Help"` のような直書きではなく、原則 `asp-page` を使う。  
仮想ディレクトリ配下の IIS で 404 を避けるためである。  
Help / Privacy のように別ウィンドウ化するリンクでも、URL 解決は `asp-page` で行い、挙動は `data-help-window` / `data-privacy-window` を使って `site.js` 側で処理する。

### 環境依存 URL は appsettings / Options に隔離する

院内専用 URL や環境依存リンクは、Razor / C# / JS に直書きしない。  
`InternalManual` のように `appsettings` / 環境変数 / Options バインディング経由で注入し、既定の `appsettings.json` では無効または空文字にしておく。  
公開リポジトリに院内専用 URL や内部向け値を含めない。

---

## テスト

- 計算ロジックの正しさは **Domain ユニットテスト** で担保する。
- UI / Index の統合テストは余力枠（`IndexIntegrationTests`）。
- 仕様の境界（年齢・肥満度・疾患・丸め）を優先的にテストで固定する。
- 実行: `dotnet test` または Visual Studio テスト エクスプローラー。

---

## ドキュメント管理

変更・判断を記録するドキュメント：

| ファイル | 用途 |
|---|---|
| `CHANGELOG.md` | バージョンごとの変更履歴 |
| `docs/ui-decisions.md` | UI 設計判断・方針の転換点 |
| `docs/appendix-*.md` | 各種付録（展開・栄養剤・トラブルシュートなど） |

UI の設計判断（CSS 調整・表示方針の変更など）は `docs/ui-decisions.md` に記録する。

---

## ロギング

Azure App Service Linux F1 プランで運用。Application Insights は使用しない。

- **最小構成**：`Program.cs` の startup try/catch と `AppDomain` / `TaskScheduler` ハンドラを維持する。
- 本番では `AddDebug()` を使わず、Console エラーを `docker.log` に出力する。
  - 確認: `grep /home/LogFiles/*docker.log`
- ログレベルは **Information 以上** を出力する（`LogLevel.Information`）。
  Error / Critical は Console 経由で `docker.log` に永続化される。
- Windows / IIS 環境では `builder.Logging.AddEventLog()` を使用する。
- ログ保持期間は 7 日間。
