using EmailAnalyzer.Application;
using MimeKit;
using System.Text;

namespace EmailAnalyzer.Infrastructure
{
    public class EmailParser : IEmailParser
    {
        public string ParseHeaders(string filePath)
        {
            var message = MimeMessage.Load(filePath);
            var headers = new StringBuilder();

            foreach (var header in message.Headers)
            {
                headers.AppendLine($"{header.Field}: {header.Value}");
            }

            return headers.ToString();
        }
    }
}
