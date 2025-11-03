#!/usr/bin/env bash
set -euo pipefail

COMPOSE_FILE="docker-compose.full.yaml"

echo "[run] Starting PostgreSQL (compose: $COMPOSE_FILE)..."
docker compose -f "$COMPOSE_FILE" up -d postgres

echo "[run] Waiting for Postgres to be ready..."
ready=0
for i in {1..60}; do
  if docker exec cartoncaps-postgres pg_isready -U postgres >/dev/null 2>&1; then
    echo "[run] Postgres is ready."
    ready=1
    break
  fi
  sleep 1
done

if [[ "$ready" -ne 1 ]]; then
  echo "[run] Postgres did not become ready in time" >&2
  exit 1
fi

echo "[run] dotnet run (API)"
dotnet run --project src/CartonCaps.Referrals.Api/CartonCaps.Referrals.Api.csproj
