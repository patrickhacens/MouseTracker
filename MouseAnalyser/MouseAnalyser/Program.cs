global using MouseAnalyser;
global using MouseTracker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MouseAnalyser.Client.Infrastructure;
using MouseAnalyser.Components;
using MouseAnalyser.Domain;
using MouseAnalyser.Infrastructure;
using MudBlazor.Services;
using System.Reflection;

[assembly: ApiController]

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
	.AddInteractiveWebAssemblyComponents();


builder.Services.AddDbContext<AnalysisDb>(op => op.UseSqlite("Data Source=analysisdb.sqlite").EnableDetailedErrors().EnableSensitiveDataLogging());

builder.Services.AddControllers();

builder.Services.AddMudServices();


builder.Services.AddScoped(sp => new HttpClient() { BaseAddress = new Uri("http://localhost:5285") });

builder.Services.AddOptions();

builder.Services.Configure<AnalysisConfig>(builder.Configuration.GetSection(nameof(AnalysisConfig)));

builder.Services.AddHostedService<HostedDatabaseLoader>();
builder.Services.AddSingleton<AnalysisObservable>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseWebAssemblyDebugging();
}
else
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
	.AddInteractiveWebAssemblyRenderMode()
	.AddAdditionalAssemblies(typeof(MouseAnalyser.Client._Imports).Assembly);

app.MapDefaultControllerRoute();

app.Run();
