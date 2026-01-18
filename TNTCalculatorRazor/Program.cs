var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.Configure<TNTCalculatorRazor.Domain.Models.InternalManualOptions>(
    builder.Configuration.GetSection("InternalManual"));

var app = builder.Build();

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
app.MapRazorPages();

app.Run();
