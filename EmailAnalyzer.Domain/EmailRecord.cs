namespace EmailAnalyzer.Domain
{
    public class EmailRecord
    {
        public required string Subject { get; set; }
        public required string From { get; set; }
        public required string RawHeaders { get; set; }
    }
}
