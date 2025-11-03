#!/usr/bin/env bash
set -euo pipefail

echo "[build] dotnet build"
dotnet build -warnaserror
