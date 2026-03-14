# Email Analyzer

Webowa aplikacja do analizy bezpieczeństwa wiadomości e-mail. Użytkownik wgrywa plik `.eml`, aplikacja parsuje nagłówki wiadomości i wysyła je do modelu AI **Google Gemini 2.5 Flash**, który zwraca szczegółowy raport bezpieczeństwa.

## Funkcjonalności

- Wgrywanie plików `.eml` przez przeglądarkę
- Parsowanie nagłówków wiadomości (Return-Path, Received, SPF, DKIM, DMARC i inne)
- Analiza AI z wyborem szablonu promptu:
  - **Security Audit** — pełny audyt cyberbezpieczeństwa z werdyktem BEZPIECZNY / PODEJRZANY / GROŹNY
  - **Simple Verdict** — krótka decyzja: bezpieczny / podejrzany
  - **Technical Route** — analiza trasy e-maila przez serwery pośredniczące
- Wynik wyświetlany w stylu terminala (animacja pisania, Markdown rendering)
- Kopiowanie raportu do schowka
- Licznik wykonanych analiz (per sesja)

## Stack technologiczny

| Komponent        | Technologia                                       |
|------------------|---------------------------------------------------|
| Runtime          | .NET 8.0                                          |
| Framework UI     | Blazor (InteractiveServer + WebAssembly)          |
| Parsing e-mail   | MimeKit 4.14.0                                    |
| Model AI         | Google Gemini 2.5 Flash (REST API)               |
| Markdown         | Markdig 0.37.0                                    |
| Konteneryzacja   | Docker (multi-stage build)                        |
| Baza danych      | brak — aplikacja bezstanowa                       |

## Architektura

Projekt stosuje **Clean Architecture** z podziałem na 4 warstwy:

```
EmailAnalyzer.Domain          # Modele domenowe (AnalysisResult, EmailRecord, AnalysisTemplate)
EmailAnalyzer.Application     # Interfejsy serwisów + AnalysisCounterService
EmailAnalyzer.Infrastructure  # Implementacje: EmailParser (MimeKit), GeminiService (HTTP)
EmailAnalyzer.WebUI           # Blazor — strony, nawigacja, UI
```

### Strony

| Ścieżka     | Opis                                                              |
|-------------|-------------------------------------------------------------------|
| `/`         | Dashboard — status systemu, model AI, licznik analiz              |
| `/analyzer` | Terminal analityczny — wgrywanie pliku, wybór trybu, wynik AI     |

## Uruchomienie

### Docker (zalecane)

```bash
git clone https://github.com/bkleparski/email-analizer.git
cd email-analizer
```

Ustaw klucz API Gemini w `docker-compose.yml` (zmienna `GeminiConfig__ApiKey`), następnie:

```bash
docker-compose up -d
```

Aplikacja dostępna pod: `http://localhost:5050`

### Lokalne środowisko (bez Dockera)

```bash
cd EmailAnalyzer.WebUI/EmailAnalyzer.WebUI
dotnet run
```

Aplikacja dostępna pod: `http://localhost:5019` lub `https://localhost:7136`

## Konfiguracja

| Zmienna środowiskowa     | Opis                                    | Przykład            |
|--------------------------|-----------------------------------------|---------------------|
| `GeminiConfig__ApiKey`   | Klucz API Google Gemini **(wymagany)**  | `AIzaSy...`         |
| `ASPNETCORE_ENVIRONMENT` | Środowisko uruchomieniowe               | `Production`        |
| `ASPNETCORE_URLS`        | Adres nasłuchiwania Kestrel             | `http://+:80`       |

> Klucze Data Protection (sesje, antiforgery) są persystowane w `./keys` na hoście i montowane do `/app/keys` w kontenerze — zapobiega to błędom po restarcie kontenera.

## Wymagania

- Docker + Docker Compose **lub** .NET 8.0 SDK
- Klucz API [Google Gemini](https://aistudio.google.com/apikey)
