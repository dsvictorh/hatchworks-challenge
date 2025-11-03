#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<EOF
Usage: $0 [--compose-file <file>] [--port <port>]

  --compose-file FILE  Compose file to use (default: docker-compose.full.yaml).
  --port PORT          Host port to probe for API health (default: 8081).

Starts the full stack (API + Postgres) with Docker Compose and waits for /health.
EOF
}

COMPOSE_FILE="docker-compose.full.yaml"
PORT=8081

while [[ $# -gt 0 ]]; do
  case "$1" in
    --compose-file)
      COMPOSE_FILE="$2"; shift 2 ;;
    --port)
      PORT="$2"; shift 2 ;;
    -h|--help)
      usage; exit 0 ;;
    *)
      echo "Unknown arg: $1"; usage; exit 1 ;;
  esac
done

if [[ ! -f "$COMPOSE_FILE" ]]; then
  echo "[run] Compose file not found: $COMPOSE_FILE" >&2
  exit 1
fi

echo "[run] Starting full stack with $COMPOSE_FILE ..."
docker compose -f "$COMPOSE_FILE" up -d
echo "[run] Waiting for API health on :$PORT ..."
for i in {1..60}; do
  if curl -fsS "http://localhost:$PORT/health" >/dev/null 2>&1; then
    echo "[run] API is healthy on http://localhost:$PORT"; break; fi
  sleep 1
done
if ! curl -fsS "http://localhost:$PORT/health" >/dev/null 2>&1; then
  echo "[run] API did not become healthy in time" >&2
  exit 1
fi
echo "[run] Open Swagger: http://localhost:$PORT/swagger"
