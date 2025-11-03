# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy central package management files early so restore resolves correct versions
COPY Directory.Packages.props ./
COPY Directory.Build.props ./

# Copy csproj files first to leverage Docker layer caching
COPY src/CartonCaps.Referrals.Api/CartonCaps.Referrals.Api.csproj src/CartonCaps.Referrals.Api/
COPY src/CartonCaps.Referrals.Core/CartonCaps.Referrals.Core.csproj src/CartonCaps.Referrals.Core/
COPY src/CartonCaps.Referrals.Infrastructure/CartonCaps.Referrals.Infrastructure.csproj src/CartonCaps.Referrals.Infrastructure/

RUN dotnet restore src/CartonCaps.Referrals.Api/CartonCaps.Referrals.Api.csproj

# Copy the rest of the source
COPY src/ ./src/

# Publish
RUN dotnet publish src/CartonCaps.Referrals.Api/CartonCaps.Referrals.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for container healthchecks
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Optional: create non-root user
RUN adduser --disabled-password --gecos "app" appuser \
    && chown -R appuser:appuser /app
USER appuser

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_EnableDiagnostics=0

EXPOSE 8081

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet","CartonCaps.Referrals.Api.dll"]
