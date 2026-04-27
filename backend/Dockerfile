# Stage 1: Builder - Kompilacja aplikacji
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder

# Ustawiamy folder roboczy w kontenerze
WORKDIR /app

# Kopiujemy cały kod backendu do kontenera
COPY . .

# Przywracamy zależności NuGet (packages)
RUN dotnet restore

# Budujemy aplikację w trybie Release (zoptymalizowany)
RUN dotnet build -c Release --no-restore

# Publikujemy (generujemy binaria gotowe do uruchomienia)
RUN dotnet publish -c Release -o /app/publish --no-build


# Stage 2: Runtime - Uruchomienie aplikacji
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Ustawiamy folder roboczy
WORKDIR /app

# Kopiujemy TYLKO opublikowane binaria z etapu builder
# (Nie potrzebujemy SDK, tylko runtime - mały rozmiar!)
COPY --from=builder /app/publish .

# Otwieramy port 5000 (na tym porcie nasłuchuje API)
EXPOSE 5000

# Ustawiamy zmienną środowiskową dla ASP.NET Core
ENV ASPNETCORE_URLS=http://+:5000

# Uruchamiamy aplikację - API.dll to skompiowana DLL naszego projektu
ENTRYPOINT ["dotnet", "API.dll"]
