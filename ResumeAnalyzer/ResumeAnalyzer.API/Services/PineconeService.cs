using ResumeAnalyzer.API.Models;
using ResumeAnalyzer.Shared.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class PineconeService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;

    public PineconeService(IConfiguration config)
    {
        _httpClient = new HttpClient();
        _apiKey = config["Pinecone:ApiKey"];
        _baseUrl = config["Pinecone:BaseUrl"];

        // Debug output
        Console.WriteLine($"API Key loaded: {!string.IsNullOrEmpty(_apiKey)} (Length: {_apiKey?.Length ?? 0})");
        Console.WriteLine($"Base URL: {_baseUrl}");
        Console.WriteLine($"API Key starts with: {_apiKey?.Substring(0, Math.Min(10, _apiKey?.Length ?? 0))}...");

        if (string.IsNullOrEmpty(_apiKey))
            throw new InvalidOperationException("Pinecone API key not found in configuration");

        if (string.IsNullOrEmpty(_baseUrl))
            throw new InvalidOperationException("Pinecone base URL not found in configuration");
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            Console.WriteLine("Testing connection to Pinecone...");
            Console.WriteLine($"Using API Key: {_apiKey?.Substring(0, Math.Min(15, _apiKey?.Length ?? 0))}...");
            Console.WriteLine($"Testing URL: {_baseUrl}/describe_index_stats");

            // Use the correct Pinecone format: Api-Key header (not Authorization)
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/describe_index_stats");
            request.Headers.Add("Api-Key", _apiKey);
            // Don't add Content-Type for GET requests

            Console.WriteLine("Making request with Api-Key header...");
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Request failed: {response.StatusCode}");
                Console.WriteLine($"Error details: {errorContent}");
                Console.WriteLine($"Response headers: {string.Join(", ", response.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}"))}");
                return false;
            }

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"✅ Connection successful! Response: {content}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection test failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return false;
        }
    }

    public async Task UpsertJobAsync(string id, float[] vector, Dictionary<string, object?> metadata)
    {
        var vectorPayload = new
        {
            vectors = new[]
            {
                new
                {
                    id = id,
                    values = vector,
                    metadata = metadata
                }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/vectors/upsert")
        {
            Content = new StringContent(JsonSerializer.Serialize(vectorPayload), Encoding.UTF8, "application/json")
        };

        // Use correct Pinecone API format
        request.Headers.Add("Api-Key", _apiKey);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"❌ Upsert failed: {response.StatusCode}");
            Console.WriteLine($"Error details: {errorContent}");
            Console.WriteLine($"Request URL: {request.RequestUri}");
            Console.WriteLine($"Request headers: {string.Join(", ", request.Headers.Select(h => $"{h.Key}: {string.Join(",", h.Value)}"))}");
            throw new HttpRequestException($"Pinecone upsert failed: {response.StatusCode} - {errorContent}");
        }
    }

    public async Task<List<JobMatchDto>> QuerySimilarJobsAsync(float[] resumeVector, int topK = 5)
    {
        var queryPayload = new
        {
            vector = resumeVector,
            topK = topK,
            includeMetadata = true
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/query")
        {
            Content = new StringContent(JsonSerializer.Serialize(queryPayload), Encoding.UTF8, "application/json")
        };

        request.Headers.Add("Api-Key", _apiKey);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Pinecone query failed: {response.StatusCode} - {errorContent}");
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var matches = doc.RootElement.GetProperty("matches");

        var results = new List<JobMatchDto>();

        foreach (var match in matches.EnumerateArray())
        {
            var metadata = match.TryGetProperty("metadata", out var meta) ? meta : default;

            results.Add(new JobMatchDto
            {
                JobId = match.GetProperty("id").GetString() ?? "unknown",
                Description = metadata.TryGetProperty("description",out var descProp) ? descProp.GetString():null,
                Score = match.GetProperty("score").GetSingle(),
                Title = metadata.TryGetProperty("title", out var titleProp) ? titleProp.GetString() ?? "unknown" : "unknown",
                Company = metadata.TryGetProperty("company", out var companyProp) ? companyProp.GetString() ?? "unknown" : "unknown",
                Location = metadata.TryGetProperty("location", out var locProp) ? locProp.GetString() ?? "unknown" : "unknown",
                Link = metadata.TryGetProperty("link", out var linkProp) ? linkProp.GetString() : null,
                MatchedSkills = metadata.TryGetProperty("skills", out var skillsProp)
                    ? (skillsProp.ToString()?.Split(',')?.Select(s => s.Trim()).ToArray() ?? [])
                    : [],
                Explanation = "" 
            });
        }

        return results;
    }

}
