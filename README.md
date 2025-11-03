# Carton Caps Referrals API

A .NET 8 REST API for handling referral codes and friend invitations with a real PostgreSQL database, migrations, validation, rate limiting, and full test coverage.

## Original Requirements Summary

This project was developed as a .NET engineering challenge.

**Challenge Context**: Design and implement REST API endpoints to power a new referral feature for the Carton Caps app, which allows users to refer friends through shareable deferred deep links.

**Deliverable #1**: REST API specification including all necessary details for both backend implementation and frontend integration, covering error states and edge cases.

**Deliverable #2**: Mock API service implementation using .NET Core with realistic fake data, comprehensive tests, and full compatibility across platforms (Windows, macOS, Linux).

## Additional requirements requested after first review (feedback)

I implemented the original challenge plus a follow-up set of production-readiness requirements (details below). Everything can run locally or fully containerized.

## What’s Included

- EF Core + Npgsql with migrations (no in-memory store)
- Feature-based minimal APIs (Referrals / Links / Events)
- Full referral lifecycle (verify → session → events click/install/open → redeem)
- FluentValidation for all request DTOs
- Centralized error handling with ProblemDetails (400/401/403/404/409/410/429)
- Rate limiting on sensitive endpoints
- Docker Compose (API + Postgres)
- 26 tests total: 17 API endpoint tests + 9 service tests

## Prerequisites

- .NET 8 SDK
- Docker Desktop (for Postgres and/or full stack)
- Bash shell for helper scripts (or run the equivalent commands manually)

## Project Structure

- `src/CartonCaps.Referrals.Api/` — Web API (Startup, endpoints, validators)
- `src/CartonCaps.Referrals.Core/` — Contracts, domain, application abstractions
- `src/CartonCaps.Referrals.Infrastructure/` — EF Core DbContext, services, migrations
- `tests/` — API + Core test projects
- `docs/` — OpenAPI (`openapi.yaml`), flows, reference and challenge files.
- `docker-compose.yaml` — Postgres only
- `docker-compose.full.yaml` — API (on 8081) + Postgres
- `scripts/` — build, run, run_full_docker, test

## Run Options

1) Local API + Postgres in Docker

- First build the project with
  - `./scripts/build.sh`
- Start DB + run API locally
  - `./scripts/run.sh`
  - Starts Postgres from `docker-compose.yaml` and runs the API with `dotnet run`.
- Ports
  - API listens on a dynamic port by default in local runs; the console will show the URL.
  - To force a port locally: `ASPNETCORE_URLS=http://localhost:5080 dotnet run --project src/CartonCaps.Referrals.Api/`

2) Full Docker (API + Postgres)

- Start everything in Docker (full stack on 8081):
  - `./scripts/run_full_docker.sh --docker`
  - Swagger: `http://localhost:8081/swagger`
  - Health: `http://localhost:8081/health`
- Alternate compose (8080):
  - `./scripts/run_full_docker.sh --docker --compose-file docker-compose.yaml --port 8080`

3) Manual commands (no scripts)

- Local build: `dotnet build CartonCaps.Referrals.sln`
- Local run: `ASPNETCORE_URLS=http://localhost:5080 dotnet run --project src/CartonCaps.Referrals.Api/`
- Full Docker: `docker compose -f docker-compose.full.yaml up -d`

### Script reference

- `scripts/build.sh` — strict local build (no containers)
- `scripts/test.sh` — start Postgres (from docker-compose.yaml) and run all tests
- `scripts/run.sh` — start Postgres (from docker-compose.full.yaml) and run API locally
- `scripts/run_full_docker.sh` — run full stack in Docker.

## Database & Migrations

Startup behavior is environment-aware (see `src/CartonCaps.Referrals.Api/Startup.cs`). By default:

- Development: applies migrations and seeds demo data on startup
- Testing: migrations/seeding handled by test host (Startup skips)
- Production: disabled by default

Use these flags to override:

- `Database:ApplyMigrationsOnStartup` — default true in Development, false otherwise
- `Database:SeedOnStartup` — default true in Development, false otherwise

Manual migration in Production (recommended):

```bash
dotnet ef database update --project src/CartonCaps.Referrals.Infrastructure --startup-project src/CartonCaps.Referrals.Api
```

## Health & Observability

- Liveness: `GET /health`
- Logging: Serilog to console + rolling files (`Logs/`), adjustable via configuration

## OpenAPI / Swagger

- Swagger UI: `/swagger`
- JSON: `/swagger/v1/swagger.json`
- Specs in repo:
  - `docs/openapi.yaml` (canonical, reconciled)
  - `openapi.yaml` (matches the current app layout)

## Testing

Quick run with Dockerized Postgres:

```bash
./scripts/test.sh
```

Or per project:

```bash
dotnet test tests/CartonCaps.Referrals.Api.Tests
dotnet test tests/CartonCaps.Referrals.Core.Tests
```

Tests reset the DB per test and disable parallelization for stability. Endpoint tests use `WebApplicationFactory` and a Testing environment.

## What Changed After Reviewer Feedback

The original submission used minimal in-memory storage and a single module. Reviewers asked for production-grade improvements. I implemented the following:

- Real DB layer using EF Core + Npgsql with migrations
- Feature-based endpoints split (Referrals/Links/Events)
- Full referral lifecycle with state transitions and idempotent event handling
- FluentValidation for all request DTOs; centralized ProblemDetails
- Broader negative tests (invalid JSON, inactive/invalid codes, duplicates, rate limiting, self-referral)
- Dockerized delivery (API + Postgres) with health checks
- Configurable startup for migrations/seeding

## Notes

- Self-referral is enforced in the service layer by comparing `RefereeUserId` with the code owner. A small guard for “user-self*” personas is applied only in Testing (to reflect test data), not in Production.
- To export the live JSON spec: `curl http://localhost:8080/swagger/v1/swagger.json` (or 8081) and convert to YAML if desired.
