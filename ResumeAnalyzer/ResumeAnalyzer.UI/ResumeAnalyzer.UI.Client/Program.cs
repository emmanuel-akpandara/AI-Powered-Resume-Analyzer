using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ResumeAnalyzer.UI.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Registering the HTTP Client
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Registering the service
builder.Services.AddScoped<ResumeService>();

await builder.Build().RunAsync();
