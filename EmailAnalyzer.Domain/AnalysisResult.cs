using System;

namespace EmailAnalyzer.Domain
{
    public class AnalysisResult
    {
        public required string FileName { get; set; }
        public DateTime AnalysisDate { get; set; }
        public required string From { get; set; }
        public required string Subject { get; set; }
        public required string AiAnalysis { get; set; }
    }
}
