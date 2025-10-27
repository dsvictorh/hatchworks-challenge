# Carton Caps Referrals API

A .NET 8 REST API for handling referral codes and friend invitations. Built as a mock service with in-memory storage.

## Original Requirements Summary

This project was developed as a .NET engineering challenge with the following core deliverables:

**Challenge Context**: Design and implement REST API endpoints to power a new referral feature for the Carton Caps app, which allows users to refer friends through shareable deferred deep links.

**Deliverable #1**: REST API specification including all necessary details for both backend implementation and frontend integration, covering error states and edge cases.

**Deliverable #2**: Mock API service implementation using .NET Core with realistic fake data, comprehensive tests, and full compatibility across platforms (Windows, macOS, Linux).

**Key Requirements**: Well-structured code following best practices, appropriate testing, error handling, edge case consideration, and proper documentation of public interfaces.

## What You Need

- **.NET 8 SDK** - Download from Microsoft
- **VS Code** or **Visual Studio Pro** (optional but recommended)
- Works on macOS, Linux, and Windows

## Project Structure

Main stuff is in:

- `src/CartonCaps.Referrals.Api/` - The web API and endpoints
- `src/CartonCaps.Referrals.Core/` - Business logic and models  
- `src/CartonCaps.Referrals.Infrastructure/` - Data storage (in-memory)
- `tests/` - Unit and integration tests

## Getting Started

Build and run from the project root:

```bash
dotnet build CartonCaps.Referrals.sln
dotnet run --project src/CartonCaps.Referrals.Api/
```

The API will be available at:

- **API**: <http://localhost:5087>
- **Swagger UI**: <http://localhost:5087/swagger>

Or use the helper scripts:

```bash
./scripts/build.sh
./scripts/run.sh
```

## Running Tests

Quick test run:

```bash
./scripts/test.sh
```

Or run them individually:

```bash
dotnet test tests/CartonCaps.Referrals.Api.Tests/
dotnet test tests/CartonCaps.Referrals.Core.Tests/
```

## Documentation

Check out the `docs/` folder:

- `openapi.yaml` - Full API spec (import into Postman/Insomnia)
- `api-endpoints-matrix.md` - Quick reference for all endpoints
- `sequence-referrals.md` - How the referral flow works

Also, the original challeng material is stored in: `docs/challenge_original_files`

## Additional Deliverables & Bonus Features

As bonus deliverables beyond the core requirements, I've included:

- **Serilog Integration**: Implemented structured logging using Serilog, which is an industry standard for .NET applications. Logs are written to both console and rotating files with proper log levels and structured data.

- **Swagger Documentation**: Added comprehensive API documentation with Swagger/OpenAPI specification. This provides interactive API documentation that's very common in professional projects I've worked on, making the API easy to explore and test.
