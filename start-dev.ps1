<#
.SYNOPSIS
    Starts the backend API, waits until it's actually healthy, then starts the frontend.

.DESCRIPTION
    1. Launches the ASP.NET Core backend (dotnet run) in its own window.
    2. Polls GET /health until the API responds 200 OK (or times out).
    3. Only once the backend is confirmed up does it install (if needed) and
       start the frontend dev server, in this window.

    Stopping: Ctrl+C here stops the frontend. The backend keeps running in its
    own window - close that window (or Ctrl+C in it) to stop the API too.
#>

$ErrorActionPreference = "Stop"

$repoRoot = $PSScriptRoot
$backendDir = Join-Path $repoRoot "backend\TaskManagement.Api"
$frontendDir = Join-Path $repoRoot "frontend\task-management-ui"
$healthUrl = "http://localhost:5122/health"
$frontendUrl = "http://localhost:5173"

if (-not (Test-Path $backendDir)) {
    Write-Error "Backend folder not found at $backendDir"
    exit 1
}
if (-not (Test-Path $frontendDir)) {
    Write-Error "Frontend folder not found at $frontendDir"
    exit 1
}

Write-Host "==> Starting backend (dotnet run) in a new window..." -ForegroundColor Cyan
Start-Process -FilePath "powershell" -ArgumentList @(
    "-NoExit",
    "-Command",
    "cd `"$backendDir`"; dotnet run"
) | Out-Null

Write-Host "==> Waiting for backend to become healthy at $healthUrl ..." -ForegroundColor Cyan

$maxAttempts = 30
$delaySeconds = 2
$isHealthy = $false

for ($attempt = 1; $attempt -le $maxAttempts; $attempt++) {
    try {
        $response = Invoke-WebRequest -Uri $healthUrl -UseBasicParsing -TimeoutSec 3
        if ($response.StatusCode -eq 200) {
            $isHealthy = $true
            break
        }
    }
    catch {
        # Backend not up yet (still building/starting/migrating) - keep retrying.
    }

    Write-Host "    ...not ready yet (attempt $attempt/$maxAttempts)" -ForegroundColor DarkGray
    Start-Sleep -Seconds $delaySeconds
}

if (-not $isHealthy) {
    Write-Error "Backend did not become healthy after $($maxAttempts * $delaySeconds)s. Check the backend window for errors."
    exit 1
}

Write-Host "==> Backend is healthy (http://localhost:5122, Swagger at /swagger)." -ForegroundColor Green

Push-Location $frontendDir
try {
    if (-not (Test-Path (Join-Path $frontendDir "node_modules"))) {
        Write-Host "==> Installing frontend dependencies (first run)..." -ForegroundColor Cyan
        npm install
    }

    Write-Host "==> Starting frontend (npm run dev) at $frontendUrl ..." -ForegroundColor Cyan
    Write-Host "    Press Ctrl+C to stop the frontend. Close the other window to stop the backend." -ForegroundColor DarkGray
    npm run dev
}
finally {
    Pop-Location
}
