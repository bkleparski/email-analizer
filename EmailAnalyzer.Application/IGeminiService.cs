using EmailAnalyzer.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmailAnalyzer.Application
{
    public interface IGeminiService
    {
        List<AnalysisTemplate> DefaultTemplates { get; }
        Task<string> AnalyzeHeaders(string rawHeaders, AnalysisTemplate template);
    }
}
