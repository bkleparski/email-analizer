using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EmailAnalyzer.Application;
using EmailAnalyzer.Domain;
using Microsoft.Extensions.Configuration;

namespace EmailAnalyzer.Infrastructure
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public List<AnalysisTemplate> DefaultTemplates { get; } = new List<AnalysisTemplate>
        {
            new AnalysisTemplate
            {
                Name = "Security Audit",
                SystemPrompt = "Jesteś ekspertem ds. cyberbezpieczeństwa i analizy śledczej nagłówków e-mail. " +
                               "Przeprowadź rygorystyczny audyt dostarczonych nagłówków.\n\n" +
                               "INSTRUKCJE:\n" +
                               "1. Sprawdź 'Return-Path' vs 'From' — różnice = ryzyko.\n" +
                               "2. Przeanalizuj sekcje 'Received' — anomalie w hostach i IP.\n" +
                               "3. Zweryfikuj Authentication-Results (SPF/DKIM/DMARC).\n" +
                               "4. Szukaj Email Spoofing i Display Name Spoofing.\n" +
                               "5. Wystaw werdykt: [BEZPIECZNY / PODEJRZANY / GROŹNY].\n" +
                               "6. Długie wartości nagłówków (DKIM-Signature, X-Microsoft-Antispam) skróć do 50 znaków + '[...]'.\n\n" +
                               "FORMAT: Markdown. Tabela parametrów technicznych, punkty dla wniosków, sekcja 'REKOMENDACJA'."
            },
            new AnalysisTemplate
            {
                Name = "Simple Verdict",
                SystemPrompt = "Jesteś analitykiem bezpieczeństwa. Na podstawie tych nagłówków, wydaj krótką decyzję: Bezpieczny lub Podejrzany."
            },
            new AnalysisTemplate
            {
                Name = "Technical Route",
                SystemPrompt = "Jesteś inżynierem sieciowym. Przeanalizuj nagłówki 'Received' i opisz trasę e-maila przez serwery pośredniczące."
            }
        };

        public GeminiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GeminiConfig:ApiKey"] ?? string.Empty;
        }

        public async Task<string> AnalyzeHeaders(string rawHeaders, AnalysisTemplate template)
        {
            var requestUri = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

            var requestBody = new
            {
                system_instruction = new { parts = new[] { new { text = template.SystemPrompt } } },
                contents = new[] { new { parts = new[] { new { text = rawHeaders } } } }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(requestUri, content);

            if (!response.IsSuccessStatusCode)
                return $"Błąd API: {response.StatusCode}";

            var responseContent = await response.Content.ReadAsStringAsync();
            var parsed = JsonDocument.Parse(responseContent);
            return parsed.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? string.Empty;
        }
    }
}
