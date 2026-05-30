using EmailAnalyzer.Domain;

namespace EmailAnalyzer.Application
{
    public interface IEmailParser
    {
        EmailRecord Parse(Stream stream);
    }
}
