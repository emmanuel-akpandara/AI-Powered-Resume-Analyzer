using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using ResumeAnalyzer.Shared.Models;

namespace ResumeAnalyzer.UI.Client.Services
{
    public class ResumeService
    {
        private readonly HttpClient _http;

        public ResumeService(HttpClient http) => _http = http;

        public async Task<List<JobMatchDto>> UploadResumeAsync(MultipartFormDataContent content)
        {
            try
            {
                // Set timeout if needed (optional)
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));

                var response = await _http.PostAsync("api/resume/match", content, cts.Token);

                // Check if the response is successful
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException(
                        $"Server returned {response.StatusCode}: {response.ReasonPhrase}. Details: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                // Handle empty response
                if (string.IsNullOrWhiteSpace(responseContent))
                {
                    return new List<JobMatchDto>();
                }

                // Try to deserialize the response
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var matchResponse = JsonSerializer.Deserialize<MatchResponse>(responseContent, options);
                return matchResponse?.Matches ?? new List<JobMatchDto>();
            }
            catch (JsonException jsonEx)
            {
                throw new InvalidOperationException($"Failed to parse server response: {jsonEx.Message}", jsonEx);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                throw new TimeoutException("The request timed out. Please try again.");
            }
            catch (HttpRequestException)
            {
                // Re-throw HTTP exceptions as-is
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An unexpected error occurred: {ex.Message}", ex);
            }
        }

        public class MatchResponse
        {
            public string Filename { get; set; } = string.Empty;
            public int MatchCount { get; set; }
            public List<JobMatchDto> Matches { get; set; } = new();
        }
    }
}