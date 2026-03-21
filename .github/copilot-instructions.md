# Copilot Instructions

## General

- If First-chance exceptions appear in Visual Studio but do not affect the application, ignore them; they are Visual Studio IDE behavior.

---

## Coding Style

### Enum-based static method pattern

Factor tables and lookup logic use `static` classes with a single `Get(EnumType)` method and a `switch` expression. Every unhandled enum value must throw `ArgumentOutOfRangeException`.

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

### Default factor values

- **積算係数（multiplier factor）** のデフォルト値は `1.0`
- **加算係数（additive factor）** のデフォルト値は `0`

---

## IE11 Compatibility

IE11 サポートが必須。CSS 新機能（CSS Grid の一部、`gap` 等）は `@supports` ガードで包み、IE 側フォールバックを先に記述する。

```css
/* IE fallback — flex で横並び、狭い画面は flex-direction: column で縦積み */
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

---

## Testing

- 計算ロジックの正しさは **Domain ユニットテスト** で担保する。
- UI / Index の統合テストは余力枠（`IndexIntegrationTests`）。
- 仕様の境界（年齢・肥満度・疾患・丸め）を優先的にテストで固定する。
- 実行: `dotnet test` または Visual Studio テスト エクスプローラー。

---

## Documentation

変更・判断を記録するドキュメント：

| ファイル | 用途 |
|---|---|
| `CHANGELOG.md` | バージョンごとの変更履歴 |
| `docs/ui-decisions.md` | UI 設計判断・方針の転換点 |
| `docs/appendix-*.md` | 各種付録（展開・栄養剤・トラブルシュートなど） |

UI の設計判断（CSS 調整・表示方針の変更など）は `docs/ui-decisions.md` に記録する。

---

## Logging

Azure App Service Linux F1 プランで運用。Application Insights は使用しない。

- **最小構成**：`Program.cs` の startup try/catch と `AppDomain` / `TaskScheduler` ハンドラを維持する。
- 本番では `AddDebug()` を使わず、Console エラーを `docker.log` に出力する。
  - 確認: `grep /home/LogFiles/*docker.log`
- ログレベルは **Error 以上のみ** を永続化する（`LogLevel.Error`）。
- Windows / IIS 環境では `builder.Logging.AddEventLog()` を使用する。
- ログ保持期間は 7 日間。
