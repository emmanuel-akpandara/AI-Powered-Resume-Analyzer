using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ResumeAnalyzer.API.Services
{
    public class ExplanationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public ExplanationService(IConfiguration config)
        {
            _apiKey = config["OpenAI:Key"];
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.openai.com/v1/")
            };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }
        public async Task<string> GenerateAsync(string resumeText, string jobText)
        {
            var prompt = $"""
            You're an AI assistant helping candidates understand job matches.

            Resume:
            {resumeText}

            Job Posting:
            {jobText}

            Briefly explain why this job matches the candidate’s resume, in 2–3 sentences. Focus on skills, technologies, and experience overlap.
            """;

            var payload = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = "You generate match explanations between resumes and job postings." },
                    new { role = "user", content = prompt }
                },
                max_tokens = 150,
                temperature = 0.7
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("chat/completions", content);
            if (!response.IsSuccessStatusCode)
                return $"Explanation unavailable: {response.StatusCode}";

            var responseBody = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseBody);

            return doc.RootElement
                     .GetProperty("choices")[0]
                     .GetProperty("message")
                     .GetProperty("content")
                     .GetString()?
                     .Trim() ?? "(No explanation returned)";
        }
    }
}