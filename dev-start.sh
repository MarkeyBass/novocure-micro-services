#!/bin/bash
# Run from the repo root: ./dev-start.sh

REPO_ROOT="$(cd "$(dirname "$0")" && pwd)"

# --- 1. Infra (Docker) ---
echo "Starting infra services..."
docker compose -f "$REPO_ROOT/docker-compose.yml" \
  up -d mongo mongo-express sqlserver cloudbeaver rabbitmq

# --- 2. Open each API in a new Terminal window ---
open_in_terminal() {
  local name="$1"
  local dir="$2"
  local cmd="$3"

  osascript \
    -e "tell application \"Terminal\"" \
    -e "  activate" \
    -e "  set w to do script \"echo '=== $name ==='; cd $dir && $cmd\"" \
    -e "  set custom title of (front window) to \"$name\"" \
    -e "end tell"
}

open_in_terminal "TodoApi"      "$REPO_ROOT/TodoApi"      "dotnet run --launch-profile https"
open_in_terminal "HousingApi"   "$REPO_ROOT/HousingApi"   "dotnet run --launch-profile https"
open_in_terminal "BookStoreApi" "$REPO_ROOT/BookStoreApi" "dotnet run --launch-profile https"

echo "Done. Three Terminal windows opened."
