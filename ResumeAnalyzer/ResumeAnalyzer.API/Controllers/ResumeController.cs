using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UglyToad.PdfPig;
using System.Text;
using ResumeAnalyzer.API.Models;
using ResumeAnalyzer.API.Services;

namespace ResumeAnalyzer.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResumeController : ControllerBase
    {
        private readonly EmbeddingService _embeddingService;
        private readonly PineconeService _pineconeService;
        private readonly ExplanationService _explanationService;

        public ResumeController(
            EmbeddingService embeddingService,
            PineconeService pineconeService,
            ExplanationService explanationService
        )
        {
            _embeddingService = embeddingService;
            _pineconeService = pineconeService;
            _explanationService = explanationService;
        }

        [HttpPost("upload")]
        public IActionResult Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var sb = new StringBuilder();
            using var stream = file.OpenReadStream();
            using var pdf = PdfDocument.Open(stream);
            foreach (var page in pdf.GetPages())
                sb.AppendLine(page.Text);

            return Ok(new { Filename = file.FileName, Content = sb.ToString() });
        }

        [HttpPost("match")]
        public async Task<IActionResult> MatchResume(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var sb = new StringBuilder();
            using var stream = file.OpenReadStream();
            using var pdf = PdfDocument.Open(stream);
            foreach (var page in pdf.GetPages())
                sb.AppendLine(page.Text);

            var resumeText = sb.ToString();
            if (string.IsNullOrWhiteSpace(resumeText))
                return BadRequest("Resume appears empty or unreadable.");

            var vector = await _embeddingService.GetEmbeddingAsync(resumeText);
            var matches = await _pineconeService.QuerySimilarJobsAsync(vector, topK: 10);

            foreach (var match in matches)
            {
                // You might want to pass a real description if you stored it
                var jobPrompt = $"{match.Title} at {match.Company}, located in {match.Location}";
                match.Explanation = await _explanationService.GenerateAsync(resumeText, match.Description);
            }

            return Ok(new
            {
                Filename = file.FileName,
                MatchCount = matches.Count,
                Matches = matches
            });
        }

        [HttpPost("match-text")]
        public async Task<IActionResult> MatchFromText([FromBody] ResumeTextModel input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.Text))
                return BadRequest("Resume text is missing.");

            var vector = await _embeddingService.GetEmbeddingAsync(input.Text);
            var matches = await _pineconeService.QuerySimilarJobsAsync(vector, topK: 10);

            foreach (var match in matches)
            {
                var jobPrompt = $"{match.Title} at {match.Company}, located in {match.Location}";
                match.Explanation = await _explanationService.GenerateAsync(input.Text, jobPrompt);
            }

            return Ok(new
            {
                Source = "raw_text",
                MatchCount = matches.Count,
                Matches = matches
            });
        }
    }

    public class ResumeTextModel
    {
        public string Text { get; set; }
    }
}
