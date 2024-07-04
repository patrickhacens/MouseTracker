using Microsoft.Extensions.DependencyInjection;
using MouseAnalyser;
using MouseAnalyser.Components;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
	.AddInteractiveWebAssemblyComponents();

builder.Services.AddControllers();

builder.Services.AddMudServices();

builder.Services.AddScoped(sp => new HttpClient() { BaseAddress = new Uri("localhost:5285") });

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
