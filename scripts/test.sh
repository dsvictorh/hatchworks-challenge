#!/usr/bin/env bash
set -euo pipefail

COMPOSE_FILE="docker-compose.yaml"

echo "[test] Starting PostgreSQL (compose: $COMPOSE_FILE)..."
docker compose -f "$COMPOSE_FILE" up -d postgres

echo "[test] Waiting for Postgres to be ready..."
ready=0
for i in {1..60}; do
  if docker exec cartoncaps-postgres pg_isready -U postgres >/dev/null 2>&1; then
    echo "[test] Postgres is ready."
    ready=1
    break
  fi
  sleep 1
done

if [[ "$ready" -ne 1 ]]; then
  echo "[test] Postgres did not become ready in time" >&2
  exit 1
fi

echo "[test] dotnet test"
dotnet test --collect:"XPlat Code Coverage"
