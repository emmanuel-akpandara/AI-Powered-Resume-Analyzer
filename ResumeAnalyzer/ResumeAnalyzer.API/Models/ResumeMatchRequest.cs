namespace ResumeAnalyzer.API.Models
{
    public class ResumeMatchRequest
    {
        public string Text { get; set; }
        public int? TopK { get; set; } = 5;
    }
}
