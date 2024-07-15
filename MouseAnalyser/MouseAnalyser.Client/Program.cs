using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MouseAnalyser.Client.Infrastructure;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);


builder.Services.AddMudServices();

builder.Services.AddSingleton<AnalysisObservable>();

builder.Services.AddScoped<HttpClient>(sp => new HttpClient()
{
	BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});


await builder.Build().RunAsync();
