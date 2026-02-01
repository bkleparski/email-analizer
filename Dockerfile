# Etap 1: Budowanie aplikacji
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopiowanie plików projektów i przywracanie zależności
COPY ["EmailAnalyzer.sln", "."]
COPY ["EmailAnalyzer.Application/EmailAnalyzer.Application.csproj", "EmailAnalyzer.Application/"]
COPY ["EmailAnalyzer.Domain/EmailAnalyzer.Domain.csproj", "EmailAnalyzer.Domain/"]
COPY ["EmailAnalyzer.Infrastructure/EmailAnalyzer.Infrastructure.csproj", "EmailAnalyzer.Infrastructure/"]
COPY ["EmailAnalyzer.WebUI/EmailAnalyzer.WebUI/EmailAnalyzer.WebUI.csproj", "EmailAnalyzer.WebUI/EmailAnalyzer.WebUI/"]
COPY ["EmailAnalyzer.WebUI/EmailAnalyzer.WebUI.Client/EmailAnalyzer.WebUI.Client.csproj", "EmailAnalyzer.WebUI/EmailAnalyzer.WebUI.Client/"]
RUN dotnet restore "EmailAnalyzer.sln"

# Kopiowanie reszty plików źródłowych
COPY . .
WORKDIR "/src/EmailAnalyzer.WebUI/EmailAnalyzer.WebUI"
RUN dotnet build "EmailAnalyzer.WebUI.csproj" -c Release -o /app/build

# Publikowanie aplikacji
FROM build AS publish
RUN dotnet publish "EmailAnalyzer.WebUI.csproj" -c Release -o /app/publish

# Etap 2: Finalny obraz
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Ustawienie zmiennej środowiskowej i punktu wejścia
ENV ASPNETCORE_URLS=http://+:80
RUN mkdir -p /app/keys && chmod 777 /app/keys
ENTRYPOINT ["dotnet", "EmailAnalyzer.WebUI.dll"]
