# Local Dev Startup

Spins up all infra in Docker and opens each API in its own Terminal window.

## Usage

From the repo root:

```bash
./dev-start.sh
```

## What it does

| Step | What happens |
|------|-------------|
| 1 | `docker compose up -d` for infra services only |
| 2 | Opens a new Terminal window for **TodoApi** (`https://localhost:7236`) |
| 3 | Opens a new Terminal window for **HousingApi** (`https://localhost:7152`) |
| 4 | Opens a new Terminal window for **BookStoreApi** (`https://localhost:7153`) |

## Infra services started

| Service | URL |
|---------|-----|
| SQL Server | `localhost:1433` |
| MongoDB | `localhost:27018` |
| Mongo Express | http://localhost:8081 |
| CloudBeaver | http://localhost:8978 |
| RabbitMQ | http://localhost:15672 (admin / admin) |

## Stop infra

```bash
docker compose down
```

## Notes

- Requires macOS Terminal.app (uses `osascript`). iTerm2 users: replace `osascript` block with `osascript -e 'tell application "iTerm2" ...'` or use the iTerm2 variant below.
- Script must be run from the repo root, or use the full path.
- .NET SDK must be installed for the target framework of each project (`dotnet --list-sdks`).

## iTerm2 variant (optional)

Replace the `open_in_terminal` function body in `dev-start.sh` with:

```bash
open_in_terminal() {
  local name="$1"
  local dir="$2"
  local cmd="$3"

  osascript \
    -e "tell application \"iTerm2\"" \
    -e "  activate" \
    -e "  set w to (create window with default profile)" \
    -e "  tell current session of w" \
    -e "    write text \"echo '=== $name ==='; cd $dir && $cmd\"" \
    -e "  end tell" \
    -e "end tell"
}
```
