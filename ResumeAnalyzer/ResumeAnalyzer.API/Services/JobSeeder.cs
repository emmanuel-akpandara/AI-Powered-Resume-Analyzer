using ClosedXML.Excel;

public class JobSeeder
{
    private readonly EmbeddingService _embeddingService;
    private readonly PineconeService _pineconeService;

    public JobSeeder(EmbeddingService embeddingService, PineconeService pineconeService)
    {
        _embeddingService = embeddingService;
        _pineconeService = pineconeService;
    }

    public async Task SeedJobsAsync(string excelPath)
    {
        // TEST CONNECTION
        Console.WriteLine("Testing Pinecone connection...");
        var connectionTest = await _pineconeService.TestConnectionAsync();
        if (!connectionTest)
        {
            Console.WriteLine("All authentication methods failed. Check your API key in Pinecone console.");
            Console.WriteLine("Common fixes:");
            Console.WriteLine("   - Verify API key is correct and active");
            Console.WriteLine("   - Check if key has write permissions");
            Console.WriteLine("   - Try creating a new API key");
            return;
        }
        Console.WriteLine("Pinecone connection successful! Starting seeding process...");

        using var workbook = new XLWorkbook(excelPath);
        var sheet = workbook.Worksheets.FirstOrDefault();
        if (sheet == null)
        {
            Console.WriteLine("No sheet found in workbook.");
            return;
        }

        var rows = sheet.RangeUsed().RowsUsed().Skip(1); // Skip header row
        int counter = 0;

        foreach (var row in rows)
        {
            try
            {
                var id = Guid.NewGuid().ToString();
                // Extracting relevant fields
                var title = row.Cell("AI").GetString(); // Example: title
                var description = row.Cell("R").GetString(); // descriptionText
                var industry = row.Cell("U").GetString(); // industries
                var jobFunction = row.Cell("W").GetString(); // jobFunction
                var seniority = row.Cell("AH").GetString(); // seniorityLevel
                var employmentType = row.Cell("S").GetString(); // employmentType
                var benefits = row.Cell("C").GetString(); // benefits/0
                var companyName = row.Cell("N").GetString();
                var companyDescription = row.Cell("J").GetString();
                var jobLocation = row.Cell("AK").GetString();
                var salary = row.Cell("AF").GetString();
                var postedAt = row.Cell("AD").GetString();
                var link = row.Cell("AB").GetString(); // LinkedIn job link

                
                string combinedText = $@"
Title: {title}
Industry: {industry}
Function: {jobFunction}
Seniority: {seniority}
Employment Type: {employmentType}
Benefits: {benefits}
Description: {description}";

                var vector = await _embeddingService.GetEmbeddingAsync(combinedText);

                // useful display metadata and also showing in the front end
                var metadata = new Dictionary<string, object?>
                {
                    ["title"] = title,
                    ["company"] = companyName,
                    ["location"] = jobLocation,
                    ["industry"] = industry,
                    ["jobFunction"] = jobFunction,
                    ["seniority"] = seniority,
                    ["employmentType"] = employmentType,
                    ["salary"] = salary,
                    ["postedAt"] = postedAt,
                    ["description"] = description,
                    ["companyDescription"] = companyDescription,
                    ["link"] = link
                };

                await _pineconeService.UpsertJobAsync(id, vector, metadata);
                Console.WriteLine($"OK! Upserted: {title} at {companyName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Nope! Skipped row due to error: {ex.Message}");
            }

            counter++;
            if (counter % 25 == 0)
            {
                Console.WriteLine($"Progress: {counter} jobs uploaded...");
                await Task.Delay(1000);
            }
        }

        Console.WriteLine($"Done! Seeded {counter} enriched jobs into Pinecone.");
    }
}
