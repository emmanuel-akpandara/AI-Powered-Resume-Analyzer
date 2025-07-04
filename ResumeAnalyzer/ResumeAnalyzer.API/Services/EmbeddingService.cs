using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class EmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public EmbeddingService(IConfiguration config)
    {
        _httpClient = new HttpClient();
        _apiKey = config["OpenAI:Key"];
    }

    public async Task<float[]> GetEmbeddingAsync(string input)
    {
        var payload = new
        {
            model = "text-embedding-3-small",
            input = input
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/embeddings");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        var json = await JsonDocument.ParseAsync(stream);

        var embedding = json.RootElement
            .GetProperty("data")[0]
            .GetProperty("embedding")
            .EnumerateArray()
            .Select(v => v.GetSingle()) // For floats
            .ToArray();

        return embedding;
    }
}
