using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// 必要最小限のロギング（コンソールとデバッグに出力）
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
// デバッグ出力は開発環境のみ(Azure App Service のログストリームに影響を与えないようにするため)
if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddDebug();
}
// Windows の場合はイベントログに出力
if (OperatingSystem.IsWindows())
{
    builder.Logging.AddEventLog();
}
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.Configure<TNTCalculatorRazor.Domain.Models.InternalManualOptions>(
    builder.Configuration.GetSection("InternalManual"));

var app = builder.Build();

// ここで ILogger を取得しておく（起動時の致命例を残すため）
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// 未処理例外 / 非同期の未観測例外を捕捉してログに残す（最小限）
AppDomain.CurrentDomain.UnhandledException += (s, e) =>
{
    try
    {
        logger.LogCritical(e.ExceptionObject as Exception, "Unhandled exception (AppDomain)");
    }
    catch
    {
        // ロガーが使えない場合でも最低限出力
        Console.Error.WriteLine("Unhandled exception (AppDomain): " + e.ExceptionObject);
    }
};
TaskScheduler.UnobservedTaskException += (s, e) =>
{
    try
    {
        logger.LogError(e.Exception, "Unobserved task exception");
    }
    catch
    {
        Console.Error.WriteLine("Unobserved task exception: " + e.Exception);
    }
    e.SetObserved();
};

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// 静的ファイルは従来方式で確実に配信 ※Production環境でsite.cssが配信されない問題への対処
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Static Assets 方式は使わない
// 監視・Azureスリープ回避用（UptimeRobot監視にGETとHEADの両方を許可）: 軽量に 200 OK を返す　※効果はhttps://<app>/favicon.ico監視と同等
app.MapMethods("/ping", new[] { "GET", "HEAD" }, () => Results.Text("OK", "text/plain"));
app.MapRazorPages();

// 起動ログを出力(テスト用)
//logger.LogInformation("Application started. Environment={env}", app.Environment.EnvironmentName);

try
{
    app.Run();
}
catch (Exception ex)
{
    // 起動時の致命的例外は必ず出力
    try
    {
        logger.LogCritical(ex, "Host terminated unexpectedly");
    }
    catch
    {
        Console.Error.WriteLine("Host terminated unexpectedly: " + ex);
    }
    throw;
}
