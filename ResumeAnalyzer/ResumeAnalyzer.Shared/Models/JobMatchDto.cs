using System.Globalization;

namespace ResumeAnalyzer.Shared.Models
{
    public class JobMatchDto
    {
        public string JobId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public float Score { get; set; }
        public string Company { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string[] MatchedSkills { get; set; } = Array.Empty<string>();
        public string Explanation { get; set; } = string.Empty;
    }
}