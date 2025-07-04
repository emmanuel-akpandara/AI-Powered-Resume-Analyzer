

using ResumeAnalyzer.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register your services
builder.Services.AddSingleton<OpenAIAnalyzer>();
builder.Services.AddSingleton<EmbeddingService>();
builder.Services.AddSingleton<PineconeService>();
builder.Services.AddSingleton<JobSeeder>(); // So it can be injected and reused
builder.Services.AddSingleton<ExplanationService>();

var app = builder.Build();

// Optional: Uncomment to run seeder manually once (recommended for CLI tool or first-time setup)
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<JobSeeder>();

    var excelPath = @"C:\Users\teste\Documents\3rd Year\dotnet\personalproject\ResumeAnalyzer\ResumeAnalyzer.API\Dataset\Linkedin_Jobs_DS.xlsx";


    // Lazy safeguard: You can add real logic here to check if index is already seeded
    var shouldSeed = false;

    if (shouldSeed)
    {
        await seeder.SeedJobsAsync(excelPath);
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
