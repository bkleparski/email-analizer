namespace EmailAnalyzer.Application
{
    public interface IEmailParser
    {
        string ParseHeaders(string filePath);
    }
}
