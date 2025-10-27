#!/usr/bin/env bash
set -euo pipefail
dotnet test --collect:"XPlat Code Coverage"
