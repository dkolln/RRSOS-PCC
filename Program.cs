using RRSOS_PCC.Classes;
using RRSOS_PCC.Components;
using RRSOS_PCC.Models;
using RRSOS_PCC.Services;
using System.Diagnostics;
using System.Runtime;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<SaveSettings>(
    builder.Configuration.GetSection("SaveSettings"));

builder.Services.AddSingleton<RRSOS_PCC.Services.SaveService>();
builder.Services.AddScoped<RRSOS_PCC.Services.GameMathService>();
builder.Services.AddSingleton<RRSOS_PCC.Services.BaseNamingService>();
builder.Services.AddSingleton<RRSOS_PCC.Services.WorldObjectClassifierService>();
builder.Services.AddSingleton<PowerService>();
builder.Services.AddSingleton<ResupplyService>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddSingleton<PCLauncherService>();
builder.Services.AddSingleton<NotebookService>();


PathResolver.Initialize(builder.Configuration, builder.Environment.ContentRootPath);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseStaticFiles();

// Required for interactive server components
app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();



