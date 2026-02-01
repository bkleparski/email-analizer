using System;
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
        private readonly string? _apiKey;

        public List<AnalysisTemplate> DefaultTemplates { get; } = new List<AnalysisTemplate>
        {
            new AnalysisTemplate
            {
                 Name = "Security Audit",
                 SystemPrompt = "Jesteś ekspertem ds. cyberbezpieczeństwa i analizy śledczej nagłówków e-mail. \nTwoim zadaniem jest przeprowadzenie rygorystycznego audytu dostarczonych nagłówków.\n\nSZCZEGÓŁOWE INSTRUKCJE:\n1. Sprawdź 'Return-Path' i porównaj go z adresem 'From'. Jeśli się różnią, zaznacz to jako ryzyko.\n2. Przeanalizuj sekcje 'Received'. Wykryj wszelkie anomalie w nazwach hostów i adresach IP.\n3. Zweryfikuj wyniki Authentication-Results. Wyjaśnij laikowi, co oznacza status 'fail' lub 'softfail' dla SPF/DKIM.\n4. Szukaj śladów 'Email Spoofing' oraz 'Display Name Spoofing'.\n5. Na podstawie analizy wystaw werdykt w skali: [BEZPIECZNY / PODEJRZANY / GROŹNY].\n6. Podczas generowania tabeli z analizą techniczną, jeśli wartość nagłówka (np. DKIM-Signature, X-Microsoft-Antispam) jest bardzo długa, nie wypisuj jej w całości. Skróć ją do pierwszych 50 znaków i dodaj '[...]'. Skup się na analizie i wyniku, a nie na przepisywaniu surowych danych nagłówka.\n\nFORMAT ODPOWIEDZI:\nOdpowiadaj w formacie Markdown. Użyj tabeli do wypisania technicznych parametrów i punktową listę dla wniosków. Na końcu dodaj sekcję 'REKOMENDACJA' dla użytkownika."
             },
            new AnalysisTemplate
            {
                Name = "Simple Verdict",
                SystemPrompt = "Jesteś analitykiem bezpieczeństwa. Na podstawie tych nagłówków, wydaj krótką decyzję: Bezpieczny lub Podejrzany."
            },
            new AnalysisTemplate
            {
                Name = "Technical Route",
                SystemPrompt = "Jesteś inżynierem sieciowym. Przeanalizuj nagłówki 'Received' i opisz trasę, jaką przebył ten e-mail, wymieniając serwery pośredniczące."
            }
        };

        public GeminiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GeminiConfig:ApiKey"] ?? string.Empty;
        }

        public async Task<string> AnalyzeHeaders(string rawHeaders, AnalysisTemplate template)
        {
            try
            {
                var requestUri = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

                var requestBody = new Dictionary<string, object>
                {
                    ["system_instruction"] = new
                    {
                        parts = new[] { new { text = template.SystemPrompt } }
                    },
                    ["contents"] = new[]
                    {
                        new
                        {
                            parts = new[] { new { text = rawHeaders } }
                        }
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(requestUri, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var parsedResponse = JsonDocument.Parse(responseContent);
                    var text = parsedResponse.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                    return text ?? string.Empty;
                }
                else
                {
                    return $"Error: {response.StatusCode}";
                }
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return "Błąd: Nie znaleziono modelu Gemini lub błąd endpointu API";
                }
                return $"Błąd połączenia: {ex.Message}";
            }
        }
    }
}
