using EmailAnalyzer.Application;
using EmailAnalyzer.Domain;
using MimeKit;
using System.Text;

namespace EmailAnalyzer.Infrastructure
{
    public class EmailParser : IEmailParser
    {
        public EmailRecord Parse(Stream stream)
        {
            var message = MimeMessage.Load(stream);
            var sb = new StringBuilder();
            foreach (var header in message.Headers)
                sb.AppendLine($"{header.Field}: {header.Value}");

            return new EmailRecord
            {
                RawHeaders = sb.ToString(),
                From = message.From.ToString(),
                Subject = message.Subject ?? "(brak tematu)"
            };
        }
    }
}
