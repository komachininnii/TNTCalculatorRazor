# =============================================================================
# Build-ChatGptBundle.ps1
# ChatGPTプロジェクトへアップロードするためのソースコードバンドルを生成するスクリプト。
# リポジトリ内の C#、Razor、CSS、JS、テスト、ドキュメントを
# カテゴリ別テキストファイルにまとめて出力する。
# =============================================================================

param(
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,  # リポジトリのルートパス
    [string]$OutputDir = "_chatgpt_bundle",                                  # 出力先ディレクトリ名
    [string]$AppProjectName = "TNTCalculatorRazor",                          # アプリプロジェクト名
    [string]$TestProjectName = "TNTCalculatorRazor.tests",                   # テストプロジェクト名
    [string]$MainPage = "Pages\Index.cshtml",                                # メインページの Razor ファイル
    [string]$MainPageModel = "Pages\Index.cshtml.cs",                        # メインページの PageModel ファイル
    [switch]$IncludeAllPages,                                                # 全ページを含めるか
    [switch]$VerboseMode                                                     # 詳細ログを出力するか
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# 情報メッセージをシアン色で出力するヘルパー
function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Cyan
}

# 警告メッセージを黄色で出力するヘルパー
function Write-WarnMsg {
    param([string]$Message)
    Write-Host "[WARN] $Message" -ForegroundColor Yellow
}

# パスを絶対パスに正規化する
function Normalize-Path {
    param([string]$Path)
    return [System.IO.Path]::GetFullPath($Path)
}

# BasePath を基準にした TargetPath の相対パスを安全に取得する
function Get-RelativePathSafe {
    param(
        [string]$BasePath,
        [string]$TargetPath
    )

    try {
        $baseUri = [System.Uri]((Normalize-Path $BasePath).TrimEnd('\') + '\')
        $targetUri = [System.Uri](Normalize-Path $TargetPath)
        $relative = $baseUri.MakeRelativeUri($targetUri).ToString()
        return [System.Uri]::UnescapeDataString($relative).Replace('/', '\')
    }
    catch {
        return $TargetPath
    }
}

# ディレクトリが存在しなければ作成する
function Ensure-Directory {
    param([string]$Path)
    if (-not (Test-Path -LiteralPath $Path)) {
        New-Item -ItemType Directory -Path $Path | Out-Null
    }
}

# git コマンドが利用可能かどうかを判定する
function Test-GitAvailable {
    try {
        $null = Get-Command git -ErrorAction Stop
        return $true
    }
    catch {
        return $false
    }
}

# リポジトリの Git 情報（ブランチ名、コミット、ステータス等）を取得する
function Get-GitInfo {
    param([string]$Root)

    $result = [ordered]@{
        IsGitRepo     = $false
        Branch        = ""
        Commit        = ""
        StatusLines   = @()
        RecentCommits = @()
    }

    if (-not (Test-GitAvailable)) {
        return $result
    }

    try {
        Push-Location $Root

        # git 出力の文字化けを防ぐため UTF-8 に統一
        try {
            $utf8 = New-Object System.Text.UTF8Encoding $false
            $OutputEncoding = $utf8
            [Console]::InputEncoding = $utf8
            [Console]::OutputEncoding = $utf8
        }
        catch {
        }

        $inside = git rev-parse --is-inside-work-tree 2>$null
        if ($LASTEXITCODE -ne 0 -or $inside -ne "true") {
            Pop-Location
            return $result
        }

        $result.IsGitRepo = $true
        $result.Branch = (git rev-parse --abbrev-ref HEAD 2>$null | Out-String).Trim()
        $result.Commit = (git rev-parse --short HEAD 2>$null | Out-String).Trim()

        $status = git -c core.quotepath=false status --short 2>$null
        if ($LASTEXITCODE -eq 0 -and $status) {
            $result.StatusLines = @($status)
        }

        $log = git -c i18n.logOutputEncoding=utf-8 -c core.quotepath=false log -n 5 --pretty=format:"%h | %ad | %s" --date=short 2>$null
        if ($LASTEXITCODE -eq 0 -and $log) {
            $result.RecentCommits = @($log)
        }

        Pop-Location
        return $result
    }
    catch {
        try { Pop-Location } catch {}
        return $result
    }
}

# 指定ディレクトリ配下から対象拡張子のファイルを再帰的に取得する（除外ディレクトリを考慮）
function Get-AllFiles {
    param(
        [string]$Root,
        [string[]]$IncludeExtensions
    )

    # バンドルに含めない中間生成物・依存パッケージ等のディレクトリ
    $excludedDirNames = @(
        "bin",
        "obj",
        ".git",
        ".vs",
        "node_modules",
        "packages",
        "artifacts",
        (Split-Path -Leaf $OutputDir)
    ) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }

    Get-ChildItem -Path $Root -Recurse -File -ErrorAction SilentlyContinue |
        Where-Object {
            $ext = $_.Extension.ToLowerInvariant()
            $IncludeExtensions -contains $ext
        } |
        Where-Object {
            $full = $_.FullName
            foreach ($dir in $excludedDirNames) {
                $token = "\" + $dir + "\"
                if ($full -like "*$token*") {
                    return $false
                }
            }
            return $true
        }
}

# 条件（Predicate）に合致するファイルをソート済みで返す
function Get-MatchingFiles {
    param(
        [string]$Root,
        [string[]]$IncludeExtensions,
        [scriptblock]$Predicate
    )

    @(
        Get-AllFiles -Root $Root -IncludeExtensions $IncludeExtensions |
            Where-Object { & $Predicate $_ } |
            Sort-Object FullName
    )
}

# 1 ファイル分の区切りヘッダーと内容を StringBuilder に追記する
function Append-FileBlock {
    param(
        [System.Text.StringBuilder]$Builder,
        [string]$RepoRootPath,
        [System.IO.FileInfo]$File
    )

    $relative = Get-RelativePathSafe -BasePath $RepoRootPath -TargetPath $File.FullName
    $lastWrite = $File.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")
    $size = $File.Length

    [void]$Builder.AppendLine("")
    [void]$Builder.AppendLine(("=" * 88))
    [void]$Builder.AppendLine("FILE: $relative")
    [void]$Builder.AppendLine("LAST WRITE: $lastWrite")
    [void]$Builder.AppendLine("SIZE: $size bytes")
    [void]$Builder.AppendLine(("=" * 88))
    [void]$Builder.AppendLine("")

    try {
        $content = Get-Content -LiteralPath $File.FullName -Raw -Encoding UTF8
    }
    catch {
        try {
            $content = Get-Content -LiteralPath $File.FullName -Raw
        }
        catch {
            $content = "[[読み込み失敗: $($_.Exception.Message)]]"
        }
    }

    [void]$Builder.AppendLine($content.TrimEnd())
    [void]$Builder.AppendLine("")
}

# BOM なし UTF-8 でテキストファイルを保存する
function Save-TextFileUtf8NoBom {
    param(
        [string]$Path,
        [string]$Content
    )

    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($Path, $Content, $utf8NoBom)
}

# =============================================
# メイン処理: パスの検証と出力先の準備
# =============================================
$solutionRoot = Normalize-Path $RepoRoot
if (-not (Test-Path -LiteralPath $solutionRoot)) {
    throw "RepoRoot が存在しません: $solutionRoot"
}

$appRoot = Join-Path $solutionRoot $AppProjectName
if (-not (Test-Path -LiteralPath $appRoot)) {
    throw "アプリ プロジェクトが見つかりません: $appRoot"
}

$testsRoot = Join-Path $solutionRoot $TestProjectName
$outDir = Join-Path $solutionRoot $OutputDir
Ensure-Directory -Path $outDir

Write-Info "SolutionRoot: $solutionRoot"
Write-Info "AppRoot: $appRoot"
if (Test-Path -LiteralPath $testsRoot) {
    Write-Info "TestsRoot: $testsRoot"
}
else {
    Write-WarnMsg "Tests project が見つかりません: $testsRoot"
}
Write-Info "OutputDir: $outDir"

$gitInfo = Get-GitInfo -Root $solutionRoot
$testFiles = @()
$docsFiles = @()

# ------------------------------------
# 1) C# Core — モデル・サービス・ヘルパー等のビジネスロジック
# ------------------------------------
$csharpFiles = Get-MatchingFiles -Root $appRoot -IncludeExtensions @(".cs") -Predicate {
    param($f)
    $rel = Get-RelativePathSafe -BasePath $appRoot -TargetPath $f.FullName

    if ($rel -match '\\(bin|obj|Migrations)\\') { return $false }
    if ($rel -match '^(Tests|test|tests)\\') { return $false }
    if ($rel -like "*.cshtml.cs") { return $false }

    if ($rel -match '^(Program\.cs|Startup\.cs)$') { return $true }

    if ($rel -match '^(Models|Model|ViewModels|Services|Domain|Enums|Extensions|Helpers|Calculators|Rules|Selectors|Utilities|Utils|PagesModels)\\') {
        return $true
    }

    if ($rel -match '^Pages\\') { return $false }

    return $false
}

$csharpBuilder = New-Object System.Text.StringBuilder
[void]$csharpBuilder.AppendLine("# 01_CSharp_Core")
[void]$csharpBuilder.AppendLine("Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
[void]$csharpBuilder.AppendLine("SolutionRoot: $solutionRoot")
[void]$csharpBuilder.AppendLine("AppRoot: $appRoot")
[void]$csharpBuilder.AppendLine("FileCount: $(@($csharpFiles).Count)")
[void]$csharpBuilder.AppendLine("")

foreach ($file in $csharpFiles) {
    if ($VerboseMode) { Write-Info "C# 追加: $($file.FullName)" }
    Append-FileBlock -Builder $csharpBuilder -RepoRootPath $solutionRoot -File $file
}

Save-TextFileUtf8NoBom -Path (Join-Path $outDir "01_CSharp_Core.txt") -Content $csharpBuilder.ToString()

# ------------------------------------
# 2) Razor UI — メインページ・共有レイアウト・ViewImports 等
# ------------------------------------
$razorFiles = @()

# メインページとその PageModel を最優先で追加
$mainPagePath = Join-Path $appRoot $MainPage
$mainPageModelPath = Join-Path $appRoot $MainPageModel

if (Test-Path -LiteralPath $mainPagePath) {
    $razorFiles += Get-Item -LiteralPath $mainPagePath
}
else {
    Write-WarnMsg "MainPage が見つかりません: $mainPagePath"
}

if (Test-Path -LiteralPath $mainPageModelPath) {
    $razorFiles += Get-Item -LiteralPath $mainPageModelPath
}
else {
    Write-WarnMsg "MainPageModel が見つかりません: $mainPageModelPath"
}

# 共有レイアウト・ViewImports・ViewStart を収集
$commonRazorFiles = Get-MatchingFiles -Root $appRoot -IncludeExtensions @(".cshtml", ".cs") -Predicate {
    param($f)
    $rel = Get-RelativePathSafe -BasePath $appRoot -TargetPath $f.FullName

    if ($rel -match '^Pages\\Shared\\') { return $true }
    if ($rel -match '^Pages\\_ViewImports\.cshtml$') { return $true }
    if ($rel -match '^Pages\\_ViewStart\.cshtml$') { return $true }

    return $false
}
$razorFiles += $commonRazorFiles

# -IncludeAllPages 指定時は Pages 配下の全 Razor ファイルを追加
if ($IncludeAllPages) {
    $allPageFiles = Get-MatchingFiles -Root $appRoot -IncludeExtensions @(".cshtml", ".cs") -Predicate {
        param($f)
        $rel = Get-RelativePathSafe -BasePath $appRoot -TargetPath $f.FullName

        if ($rel -match '^Pages\\') {
            if ($rel -match '\\(bin|obj)\\') { return $false }
            if ($rel -match '^Pages\\Shared\\') { return $false }
            if ($rel -match '\.cshtml$' -or $rel -match '\.cshtml\.cs$') { return $true }
        }

        return $false
    }
    $razorFiles += $allPageFiles
}

# 重複を排除してソート
$razorFiles = @($razorFiles | Sort-Object FullName -Unique)

$razorBuilder = New-Object System.Text.StringBuilder
[void]$razorBuilder.AppendLine("# 02_Razor_UI")
[void]$razorBuilder.AppendLine("Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
[void]$razorBuilder.AppendLine("SolutionRoot: $solutionRoot")
[void]$razorBuilder.AppendLine("AppRoot: $appRoot")
[void]$razorBuilder.AppendLine("MainPage: $MainPage")
[void]$razorBuilder.AppendLine("MainPageModel: $MainPageModel")
[void]$razorBuilder.AppendLine("FileCount: $(@($razorFiles).Count)")
[void]$razorBuilder.AppendLine("")

foreach ($file in $razorFiles) {
    if ($VerboseMode) { Write-Info "Razor 追加: $($file.FullName)" }
    Append-FileBlock -Builder $razorBuilder -RepoRootPath $solutionRoot -File $file
}

Save-TextFileUtf8NoBom -Path (Join-Path $outDir "02_Razor_UI.txt") -Content $razorBuilder.ToString()

# ------------------------------------
# 3) CSS — カスタムスタイルシート（lib 配下のサードパーティは除外）
# ------------------------------------
$cssFiles = Get-MatchingFiles -Root $appRoot -IncludeExtensions @(".css") -Predicate {
    param($f)
    $rel = Get-RelativePathSafe -BasePath $appRoot -TargetPath $f.FullName

    if ($rel -match '^wwwroot\\lib\\') { return $false }

    return ($rel -match '^wwwroot\\css\\') -or ($rel -like "*.css")
}

$cssBuilder = New-Object System.Text.StringBuilder
[void]$cssBuilder.AppendLine("# 03_Styles")
[void]$cssBuilder.AppendLine("Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
[void]$cssBuilder.AppendLine("SolutionRoot: $solutionRoot")
[void]$cssBuilder.AppendLine("AppRoot: $appRoot")
[void]$cssBuilder.AppendLine("FileCount: $(@($cssFiles).Count)")
[void]$cssBuilder.AppendLine("")

foreach ($file in $cssFiles) {
    if ($VerboseMode) { Write-Info "CSS 追加: $($file.FullName)" }
    Append-FileBlock -Builder $cssBuilder -RepoRootPath $solutionRoot -File $file
}

Save-TextFileUtf8NoBom -Path (Join-Path $outDir "03_Styles.txt") -Content $cssBuilder.ToString()

# ------------------------------------
# 4) JS — カスタムスクリプト（lib 配下のサードパーティは除外）
# ------------------------------------
$jsFiles = Get-MatchingFiles -Root $appRoot -IncludeExtensions @(".js") -Predicate {
    param($f)
    $rel = Get-RelativePathSafe -BasePath $appRoot -TargetPath $f.FullName

    if ($rel -match '^wwwroot\\lib\\') { return $false }

    return ($rel -match '^wwwroot\\js\\') -or ($rel -like "*.js")
}

$jsBuilder = New-Object System.Text.StringBuilder
[void]$jsBuilder.AppendLine("# 04_Scripts")
[void]$jsBuilder.AppendLine("Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
[void]$jsBuilder.AppendLine("SolutionRoot: $solutionRoot")
[void]$jsBuilder.AppendLine("AppRoot: $appRoot")
[void]$jsBuilder.AppendLine("FileCount: $(@($jsFiles).Count)")
[void]$jsBuilder.AppendLine("")

foreach ($file in $jsFiles) {
    if ($VerboseMode) { Write-Info "JS 追加: $($file.FullName)" }
    Append-FileBlock -Builder $jsBuilder -RepoRootPath $solutionRoot -File $file
}

Save-TextFileUtf8NoBom -Path (Join-Path $outDir "04_Scripts.txt") -Content $jsBuilder.ToString()

# ------------------------------------
# 5) Tests — テストプロジェクト内の C# ファイル
# ------------------------------------
if (Test-Path -LiteralPath $testsRoot) {
    $testFiles = Get-MatchingFiles -Root $testsRoot -IncludeExtensions @(".cs") -Predicate {
        param($f)
        $rel = Get-RelativePathSafe -BasePath $testsRoot -TargetPath $f.FullName

        if ($rel -match '\\(bin|obj)\\') { return $false }

        return $true
    }
}

if (@($testFiles).Count -gt 0) {
    $testsBuilder = New-Object System.Text.StringBuilder
    [void]$testsBuilder.AppendLine("# 05_Tests")
    [void]$testsBuilder.AppendLine("Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
    [void]$testsBuilder.AppendLine("SolutionRoot: $solutionRoot")
    [void]$testsBuilder.AppendLine("TestsRoot: $testsRoot")
    [void]$testsBuilder.AppendLine("FileCount: $(@($testFiles).Count)")
    [void]$testsBuilder.AppendLine("")

    foreach ($file in $testFiles) {
        if ($VerboseMode) { Write-Info "TEST 追加: $($file.FullName)" }
        Append-FileBlock -Builder $testsBuilder -RepoRootPath $solutionRoot -File $file
    }

    Save-TextFileUtf8NoBom -Path (Join-Path $outDir "05_Tests.txt") -Content $testsBuilder.ToString()
}

# ------------------------------------
# 6) Docs — README・CHANGELOG および docs 配下のマークダウン
# ------------------------------------
$docsRoot = Join-Path $solutionRoot "docs"

# リポジトリ直下の主要ドキュメント
$rootDocCandidates = @(
    (Join-Path $solutionRoot "README.md"),
    (Join-Path $solutionRoot "CHANGELOG.md")
    (Join-Path $solutionRoot ".github\copilot-instructions.md")  # GitHub Copilot / AI向けコーディング指示書
)

$rootDocFiles = foreach ($path in $rootDocCandidates | Select-Object -Unique) {
    if (Test-Path -LiteralPath $path) {
        Get-Item -LiteralPath $path
    }
}

$nestedDocFiles = @()
if (Test-Path -LiteralPath $docsRoot) {
    $nestedDocFiles = Get-MatchingFiles -Root $docsRoot -IncludeExtensions @(".md") -Predicate {
        param($f)
        $rel = Get-RelativePathSafe -BasePath $docsRoot -TargetPath $f.FullName
        if ($rel -match '\\(bin|obj)\\') { return $false }
        return $true
    }
}

$docsFiles = @($rootDocFiles + $nestedDocFiles | Sort-Object FullName -Unique)

if (@($docsFiles).Count -gt 0) {
    $docsBuilder = New-Object System.Text.StringBuilder
    [void]$docsBuilder.AppendLine("# 06_Docs")
    [void]$docsBuilder.AppendLine("Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
    [void]$docsBuilder.AppendLine("SolutionRoot: $solutionRoot")
    [void]$docsBuilder.AppendLine("DocsRoot: $docsRoot")
    [void]$docsBuilder.AppendLine("FileCount: $(@($docsFiles).Count)")
    [void]$docsBuilder.AppendLine("")

    foreach ($file in $docsFiles) {
        if ($VerboseMode) { Write-Info "DOC 追加: $($file.FullName)" }
        Append-FileBlock -Builder $docsBuilder -RepoRootPath $solutionRoot -File $file
    }

    Save-TextFileUtf8NoBom -Path (Join-Path $outDir "06_Docs.txt") -Content $docsBuilder.ToString()
}

# ------------------------------------
# 7) Project Map — バンドル全体の目次・Git 情報・ファイル一覧を生成
# ------------------------------------
$mapBuilder = New-Object System.Text.StringBuilder
[void]$mapBuilder.AppendLine("# 00_Project_Map")
[void]$mapBuilder.AppendLine("")
[void]$mapBuilder.AppendLine("Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
[void]$mapBuilder.AppendLine("SolutionRoot: $solutionRoot")
[void]$mapBuilder.AppendLine("AppRoot: $appRoot")
if (Test-Path -LiteralPath $testsRoot) {
    [void]$mapBuilder.AppendLine("TestsRoot: $testsRoot")
}
[void]$mapBuilder.AppendLine("OutputDir: $OutputDir")
[void]$mapBuilder.AppendLine("")

if ($gitInfo.IsGitRepo) {
    [void]$mapBuilder.AppendLine("## Git")
    [void]$mapBuilder.AppendLine("")
    [void]$mapBuilder.AppendLine("- Branch: $($gitInfo.Branch)")
    [void]$mapBuilder.AppendLine("- Commit: $($gitInfo.Commit)")
    [void]$mapBuilder.AppendLine("")

    [void]$mapBuilder.AppendLine("### Working tree status")
    [void]$mapBuilder.AppendLine("")
    if (@($gitInfo.StatusLines).Count -gt 0) {
        foreach ($line in $gitInfo.StatusLines) {
            [void]$mapBuilder.AppendLine("- $line")
        }
    }
    else {
        [void]$mapBuilder.AppendLine("- clean")
    }
    [void]$mapBuilder.AppendLine("")

    [void]$mapBuilder.AppendLine("### Recent commits")
    [void]$mapBuilder.AppendLine("")
    if (@($gitInfo.RecentCommits).Count -gt 0) {
        foreach ($line in $gitInfo.RecentCommits) {
            [void]$mapBuilder.AppendLine("- $line")
        }
    }
    [void]$mapBuilder.AppendLine("")
}

[void]$mapBuilder.AppendLine("## Bundle contents")
[void]$mapBuilder.AppendLine("")
[void]$mapBuilder.AppendLine("- 01_CSharp_Core.txt : Calculator / Selector / Rule / Enum / Model / Helper / Program など")
[void]$mapBuilder.AppendLine("- 02_Razor_UI.txt : Main Page, PageModel, Shared Layout, ViewImports など")
[void]$mapBuilder.AppendLine("- 03_Styles.txt : CSS 一式")
[void]$mapBuilder.AppendLine("- 04_Scripts.txt : JS 一式")
if (@($testFiles).Count -gt 0) {
    [void]$mapBuilder.AppendLine("- 05_Tests.txt : xUnit などのテストコード")
}
if (@($docsFiles).Count -gt 0) {
    [void]$mapBuilder.AppendLine("- 06_Docs.txt : README / CHANGELOG / .github/copilot-instructions.md / docs 配下の文書")
}
[void]$mapBuilder.AppendLine("")

[void]$mapBuilder.AppendLine("## Main page")
[void]$mapBuilder.AppendLine("")
[void]$mapBuilder.AppendLine("- MainPage: $MainPage")
[void]$mapBuilder.AppendLine("- MainPageModel: $MainPageModel")
[void]$mapBuilder.AppendLine("")

[void]$mapBuilder.AppendLine("## Suggested prompt template")
[void]$mapBuilder.AppendLine("")
[void]$mapBuilder.AppendLine("[Prompt Template]")
[void]$mapBuilder.AppendLine("現象:")
[void]$mapBuilder.AppendLine("再現手順:")
[void]$mapBuilder.AppendLine("期待動作:")
[void]$mapBuilder.AppendLine("実際の動作:")
[void]$mapBuilder.AppendLine("最近変更した箇所:")
[void]$mapBuilder.AppendLine("まず見てほしいファイル:")
[void]$mapBuilder.AppendLine("補足:")
[void]$mapBuilder.AppendLine("")

[void]$mapBuilder.AppendLine("## File list summary")
[void]$mapBuilder.AppendLine("")

[void]$mapBuilder.AppendLine("### C#")
foreach ($f in $csharpFiles) {
    $rel = Get-RelativePathSafe -BasePath $solutionRoot -TargetPath $f.FullName
    [void]$mapBuilder.AppendLine("- $rel")
}
[void]$mapBuilder.AppendLine("")

[void]$mapBuilder.AppendLine("### Razor")
foreach ($f in $razorFiles) {
    $rel = Get-RelativePathSafe -BasePath $solutionRoot -TargetPath $f.FullName
    [void]$mapBuilder.AppendLine("- $rel")
}
[void]$mapBuilder.AppendLine("")

[void]$mapBuilder.AppendLine("### CSS")
foreach ($f in $cssFiles) {
    $rel = Get-RelativePathSafe -BasePath $solutionRoot -TargetPath $f.FullName
    [void]$mapBuilder.AppendLine("- $rel")
}
[void]$mapBuilder.AppendLine("")

[void]$mapBuilder.AppendLine("### JS")
foreach ($f in $jsFiles) {
    $rel = Get-RelativePathSafe -BasePath $solutionRoot -TargetPath $f.FullName
    [void]$mapBuilder.AppendLine("- $rel")
}
[void]$mapBuilder.AppendLine("")

if (@($testFiles).Count -gt 0) {
    [void]$mapBuilder.AppendLine("### Tests")
    foreach ($f in $testFiles) {
        $rel = Get-RelativePathSafe -BasePath $solutionRoot -TargetPath $f.FullName
        [void]$mapBuilder.AppendLine("- $rel")
    }
    [void]$mapBuilder.AppendLine("")
}

if (@($docsFiles).Count -gt 0) {
    [void]$mapBuilder.AppendLine("### Docs")
    foreach ($f in $docsFiles) {
        $rel = Get-RelativePathSafe -BasePath $solutionRoot -TargetPath $f.FullName
        [void]$mapBuilder.AppendLine("- $rel")
    }
    [void]$mapBuilder.AppendLine("")
}

Save-TextFileUtf8NoBom -Path (Join-Path $outDir "00_Project_Map.md") -Content $mapBuilder.ToString()

# 完了メッセージと生成されたファイル一覧を表示
Write-Info "生成完了"
Write-Info "出力先: $outDir"
Write-Info "作成ファイル:"
Write-Host "  - 00_Project_Map.md"
Write-Host "  - 01_CSharp_Core.txt"
Write-Host "  - 02_Razor_UI.txt"
Write-Host "  - 03_Styles.txt"
Write-Host "  - 04_Scripts.txt"
if (@($testFiles).Count -gt 0) {
    Write-Host "  - 05_Tests.txt"
}
if (@($docsFiles).Count -gt 0) {
    Write-Host "  - 06_Docs.txt"
}
