//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using ResumeAnalyzer.API.Models;

//namespace ResumeAnalyzer.API.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class MatchController : ControllerBase
//    {
//        private readonly EmbeddingService _embeddingService;
//        private readonly PineconeService _pineconeService;

//        public MatchController (EmbeddingService embeddingService, PineconeService pineconeService)
//        {
//            _embeddingService = embeddingService;
//            _pineconeService = pineconeService;
//        }
//        [HttpPost("resume")]
//        public async Task<IActionResult> MatchResumeAsync([FromBody] ResumeMatchRequest request)
//        {
//            if (string.IsNullOrWhiteSpace(request.Text))
//                return BadRequest("Resume text is required.");
//            var vector = await _embeddingService.GetEmbeddingAsync(request.Text);
//            var matches = await _pineconeService.QuerySimilarJobsAsync(vector, request.TopK ?? 5);

//            return Ok(matches.Select(m => new
//            {
//                JobId = m.Id,
//                Title = m.Title,
//                Score = Math.Round(m.Score, 4)
//            }));
//        }
//    }
//}
