#!/usr/bin/env bash
#
# Starts the backend API, waits until it's actually healthy, then starts the
# frontend. Ctrl+C stops the frontend and (via the trap below) the backend
# it started.

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKEND_DIR="$REPO_ROOT/backend/TaskManagement.Api"
FRONTEND_DIR="$REPO_ROOT/frontend/task-management-ui"
HEALTH_URL="http://localhost:5122/health"
FRONTEND_URL="http://localhost:5173"

if [ ! -d "$BACKEND_DIR" ]; then
  echo "Backend folder not found at $BACKEND_DIR" >&2
  exit 1
fi
if [ ! -d "$FRONTEND_DIR" ]; then
  echo "Frontend folder not found at $FRONTEND_DIR" >&2
  exit 1
fi

BACKEND_PID=""
cleanup() {
  if [ -n "$BACKEND_PID" ] && kill -0 "$BACKEND_PID" 2>/dev/null; then
    echo "==> Stopping backend (pid $BACKEND_PID)..."
    kill "$BACKEND_PID" 2>/dev/null || true
  fi
}
trap cleanup EXIT INT TERM

echo "==> Starting backend (dotnet run)..."
(cd "$BACKEND_DIR" && dotnet run) &
BACKEND_PID=$!

echo "==> Waiting for backend to become healthy at $HEALTH_URL ..."
MAX_ATTEMPTS=30
DELAY_SECONDS=2
IS_HEALTHY=false

for attempt in $(seq 1 "$MAX_ATTEMPTS"); do
  if curl --silent --fail --max-time 3 "$HEALTH_URL" > /dev/null 2>&1; then
    IS_HEALTHY=true
    break
  fi
  echo "    ...not ready yet (attempt $attempt/$MAX_ATTEMPTS)"
  sleep "$DELAY_SECONDS"
done

if [ "$IS_HEALTHY" != "true" ]; then
  echo "Backend did not become healthy after $((MAX_ATTEMPTS * DELAY_SECONDS))s. Check the output above for errors." >&2
  exit 1
fi

echo "==> Backend is healthy (http://localhost:5122, Swagger at /swagger)."

cd "$FRONTEND_DIR"
if [ ! -d node_modules ]; then
  echo "==> Installing frontend dependencies (first run)..."
  npm install
fi

echo "==> Starting frontend (npm run dev) at $FRONTEND_URL ..."
echo "    Press Ctrl+C to stop both the frontend and the backend."
npm run dev
