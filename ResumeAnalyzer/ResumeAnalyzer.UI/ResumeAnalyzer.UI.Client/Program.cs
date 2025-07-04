using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ResumeAnalyzer.UI.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Register HTTP Client
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register service
builder.Services.AddScoped<ResumeService>();

await builder.Build().RunAsync();
