# Task Management App

A small task management API + frontend: create tasks, list them, toggle completion.
Built for the Full Stack Developer take-home test, then extended per follow-up
requests to use hexagonal architecture on both sides, EF Core Code-First
migrations with seed data, Unit of Work, NUnit/Moq on the backend, a dark/indigo
theme with a stats footer, and JWT bearer-token authentication.

## Auth: no sign-in screen, but the API is still protected

`/api/tasks` requires a valid JWT, same as before. There's just no visible sign-in
form - the frontend authenticates automatically on startup with a single demo
account (`admin` / `admin123!`, configured in
`backend/TaskManagement.Api/appsettings.json`'s `DemoUser` section) and re-authenticates
transparently if a token expires or is rejected mid-session. See the trade-offs
section for why, and what a real multi-user version would look like instead.

```
.
├── backend/
│   ├── TaskManagement.sln            solution referencing both backend projects below
│   ├── TaskManagement.Api/           ASP.NET Core 8 Web API (hexagonal, EF Core + SQLite)
│   └── TaskManagement.Api.Tests/     NUnit + Moq
├── frontend/        React + TypeScript + Vite + MUI (hexagonal, Jest + RTL)
├── start-dev.ps1    starts backend, waits for /health, then starts frontend (Windows)
└── start-dev.sh     same, for macOS/Linux/WSL
```

## Quick start

**One command (recommended):** starts the backend, waits until `/health` actually
responds, then starts the frontend.

```powershell
# Windows
.\start-dev.ps1
```

```bash
# macOS / Linux / WSL
./start-dev.sh
```

**Or run each side manually:**

**Backend** (http://localhost:5122, Swagger at `/swagger`):

```bash
cd backend
dotnet restore TaskManagement.sln
dotnet run --project TaskManagement.Api
```

(`dotnet restore`/`build` also work directly against `TaskManagement.sln` to
pick up both projects at once - see Tests below.)

The database (SQLite file `tasks.db`) is created and migrated automatically on
startup, seeded with 3 example tasks. `dotnet run` binds to `http://localhost:5122`
via `Properties/launchSettings.json` - this is the port the frontend's default
`.env.example` points at, so the two agree without any manual config.

To call protected endpoints directly (e.g. via Swagger's "Authorize" button or
curl), first get a token:

```bash
curl -X POST http://localhost:5122/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123!"}'
```

**Frontend** (http://localhost:5173):

```bash
cd frontend/task-management-ui
npm install
cp .env.example .env.local   # only needed if the API isn't on localhost:5122
npm run dev
```

**Tests:**

```bash
# Backend (whole solution)
cd backend
dotnet test TaskManagement.sln

# Frontend
cd frontend/task-management-ui
npm test
```

---

## Architecture

Both projects are organized as **hexagonal architecture (ports & adapters)**:
a framework-free core in the middle, with driving adapters (things that call
into the app) and driven adapters (things the app calls out to) on the
outside. Nothing in the core imports EF Core, ASP.NET Core, React, or fetch.

### Backend

```
TaskManagement.Api/
├── Core/
│   ├── Domain/            TaskItem — the entity, no framework attributes
│   ├── Ports/              ITaskRepository, IUnitOfWork, ITaskService,
│   │                       IAuthService, IJwtTokenGenerator (interfaces)
│   └── Application/
│       ├── TaskService.cs      business logic, depends only on IUnitOfWork
│       ├── AuthService.cs      checks credentials, delegates to IJwtTokenGenerator
│       ├── DemoUserOptions.cs  config-bound stand-in for a real user store
│       ├── Dtos/                TaskDto, CreateTaskRequest, LoginRequest, LoginResponse
│       ├── Validation/          FluentValidation for CreateTaskRequest, LoginRequest
│       └── Mapping/             TaskItem -> TaskDto
├── Adapters/
│   ├── Persistence/        TaskDbContext, TaskRepository, UnitOfWork (EF Core + SQLite)
│   ├── Security/            JwtTokenGenerator (signs tokens), JwtOptions
│   └── Http/                 TaskFunctions, AuthFunctions (Minimal API endpoints),
│                              exception middleware
├── Migrations/              hand-authored InitialCreate migration + seed data (see caveat below)
└── Program.cs                composition root / DI wiring, JWT bearer auth setup
```

- **Ports** (`ITaskRepository`, `IUnitOfWork`, `ITaskService`, `IAuthService`,
  `IJwtTokenGenerator`) are the seams. `ITaskService`/`IAuthService` are *driving*
  ports — the HTTP layer calls into them. `ITaskRepository`/`IUnitOfWork`/
  `IJwtTokenGenerator` are *driven* ports — the application core calls out through
  them, with EF Core / a JWT library as the implementation swapped in behind the interface.
- **Repository + Unit of Work.** `TaskRepository` only *stages* changes (`AddAsync`,
  `Remove`) — it never calls `SaveChanges`. `UnitOfWork.SaveChangesAsync()` is the single
  place a commit happens. This is what lets a future use case touch multiple repositories
  and commit them atomically.
- **Endpoints are thin.** `TaskFunctions`/`AuthFunctions` (Minimal API, matching the
  "functions" framing of the original ask — conceptually equivalent to REST controllers)
  validate the request DTO and call the relevant service port. No EF Core, no JWT
  library, no business logic in the endpoint itself.
- Request-shape validation (FluentValidation) happens at the HTTP adapter, *before*
  calling the service — `TaskService`/`AuthService` assume they're always given
  already-valid input. This keeps the application core free of transport-layer concerns.
- **Auth split into two ports** (`IAuthService` for the credential check, `IJwtTokenGenerator`
  for minting the token) so the business rule ("are these credentials valid") and the token
  format (JWT today, could be anything else) vary independently. `/api/tasks` is protected
  with `.RequireAuthorization()`; `/health` and `/api/auth/login` stay anonymous.

### Frontend

```
src/
├── domain/                    Task, AuthSession types + *Port interfaces
├── application/
│   ├── useCases/               listTasks, createTask, toggleTask, deleteTask, login
│   └── taskStats.ts             pure derivation (total/completed/pending) for the stats footer
├── infrastructure/
│   ├── http/                   HttpTaskRepository, HttpAuthRepository, apiClient, env.ts
│   └── auth/tokenStorage.ts    localStorage-backed token persistence
└── ui/
    ├── auth/
    │   ├── AuthContext.tsx      driving adapter: auto-authenticates on mount via the
    │   │                        login use case, re-authenticates on a 401, no visible UI
    │   └── demoCredentials.ts   the one hardcoded demo account (mirrors backend appsettings)
    ├── hooks/useTasks.ts         driving adapter: wires React state to the task use cases
    └── components/                AddTaskForm, TaskList, TaskListItem, StatsFooter (MUI, dark theme)
```

Same idea as the backend: `TaskRepositoryPort`/`AuthRepositoryPort` are the interfaces the
use cases and UI depend on. `HttpTaskRepository`/`HttpAuthRepository` are the only places
that know about `fetch` or the API's JSON shape. Swapping in a fake repository for testing,
or a different transport later, touches nothing else. `apiClient.ts` attaches the stored
bearer token to every request and, on a 401, clears it and fires a
`task-manager:unauthorized` window event - `AuthContext` listens for that and silently
re-authenticates, instead of every call site handling it individually. `App.tsx` shows a
brief "Connecting..." spinner while that first auto-login is in flight, and a "Retry"
screen if the API is unreachable - the only auth-related UI that exists now.

---

## Assumptions & trade-offs

- **No visible sign-in screen; the frontend auto-authenticates with a hardcoded demo
  account** (`ui/auth/demoCredentials.ts`). This means that account's password ships
  in the JS bundle - completely fine for a single-demo-user take-home where the point
  is demonstrating the JWT layer exists, but it is **not** how you'd do this for a real
  multi-user app. A real version would bring back a sign-in form (the removed
  `LoginForm`/`login` use case pattern is still in git history and easy to restore) and
  stop shipping any credential to the client.
- **Single demo user, no registration/user store.** `DemoUserOptions` (bound from the
  `DemoUser` config section) is a stand-in for a real Users table. There's no password
  hashing (nothing to hash - it's one hardcoded config value) and no registration flow.
  A real system would replace `AuthService`'s credential check with a DB-backed lookup
  against hashed passwords (e.g. BCrypt/ASP.NET Core Identity); nothing outside
  `AuthService` would need to change, since it's reached only through the `IAuthService` port.
- **JWT signing key lives in `appsettings.json`**, clearly marked dev-only. A real
  deployment must pull this from an environment variable or a secret manager (Azure Key
  Vault, AWS Secrets Manager, etc.) - never commit a real signing key to source control.
- **Frontend stores the token in `localStorage`**, not an httpOnly cookie. Simpler to
  implement (no cookie/CORS credential plumbing, no CSRF token needed) but readable by
  any script on the page, so it's more exposed to XSS than a cookie would be. A production
  app handling sensitive data would prefer the backend issuing an httpOnly, SameSite cookie
  instead.
- **No refresh tokens.** The token simply expires after `ExpiryMinutes` (60 by default) and
  the user has to log in again - no silent renewal. Acceptable for this scope; a longer-lived
  session would need a refresh-token flow.
- **SQLite + EF Core Code-First migrations**, chosen over EF Core InMemory so the "SQL DB"
  requirement is demonstrated with real persistence and a real migration history, not just
  an ephemeral test double.
- **Migrations were hand-authored, not `dotnet ef`-generated.** This sandbox environment
  has no outbound access to `nuget.org` / `dot.net`, so the .NET SDK couldn't be installed
  here to run `dotnet ef migrations add`. The migration files were written by hand to match
  EF Core 8's generated output as closely as possible, and the backend test suite exercises
  them for real (`TaskRepositoryTests` calls `Database.Migrate()`, not `EnsureCreated()`) —
  so a passing `dotnet test` run is a reasonably strong signal they're correct. Still, the
  **first thing to do on a machine with the SDK** is regenerate them cleanly:
  ```bash
  cd backend/TaskManagement.Api
  dotnet tool install --global dotnet-ef   # if not already installed
  rm -rf Migrations
  dotnet ef migrations add InitialCreate
  ```
- **Seed data** (3 example tasks) is baked into the migration via `HasData`, so every
  fresh clone starts from the same state with zero manual steps.
- **Validation split from the service.** `CreateTaskRequestValidator` runs in the HTTP
  adapter, not inside `TaskService`. Debatable either way; this keeps `ITaskService`
  usable from any driving adapter without re-validating transport-shape concerns it
  shouldn't know about.
- **Swagger is always on** (not gated to `Development`), so a reviewer can exercise the
  API immediately without extra config.
- **Delete task** was added beyond the spec ("add any features you feel are missing") —
  toggling and creating without ever being able to remove a task felt like an obvious gap.
- **DI registers `IUnitOfWork`/`ITaskService`, not `ITaskRepository` directly** — the
  repository is only reachable through `IUnitOfWork.Tasks`, so `SaveChanges` stays
  centralized in one place instead of being callable from just anywhere.
- **Not split into separate class-library projects** (e.g. `TaskManagement.Domain.csproj`,
  `TaskManagement.Infrastructure.csproj`). The `Core`/`Adapters` folder boundaries give the
  same enforced-by-convention separation without the ceremony of extra `.csproj` files and
  project references, which felt like the right trade-off for this scope. A larger app
  would likely promote each folder to its own project.
- **CORS** is locked to `http://localhost:5173` (the frontend's dev port) rather than
  wide open.

## Testing

- **Backend:** NUnit. `TaskServiceTests`/`AuthServiceTests` mock `IUnitOfWork`/
  `ITaskRepository`/`IJwtTokenGenerator` with Moq — pure unit tests of the business logic,
  no database, no real JWT signing. `JwtTokenGeneratorTests` validates a real generated
  token against real `TokenValidationParameters` (including a signature-mismatch case) so
  the security-critical bit isn't just mocked away. `TaskRepositoryTests` runs against a
  real in-memory SQLite connection via `Database.Migrate()`. `TaskEndpointsTests` boots the
  actual ASP.NET Core pipeline with `WebApplicationFactory` for end-to-end coverage
  (routing, auth, validation, middleware included) - it logs in via `/api/auth/login` in
  `SetUp` and attaches the bearer token to the client, plus covers the unauthenticated
  (401) and invalid-login (401) paths explicitly.
- **Frontend:** Jest + React Testing Library. Use cases are tested with hand-rolled mock
  `TaskRepositoryPort`/`AuthRepositoryPort` implementations (no HTTP, no React).
  `AuthContext` is tested by rendering it with a mock repository and asserting the
  auto-login-on-mount, error + retry, and re-authenticate-on-401 flows through a small
  consumer component. Components are tested through user-facing behavior (clicking,
  typing) rather than implementation details.
- Given the 1–1.5 hour framing of the original brief, coverage is deliberately focused on
  the core flows (create/list/toggle/delete, login/logout, validation, not-found,
  unauthorized) rather than exhaustive edge cases.

## Future improvements

- Filter tasks by status (index on `IsCompleted` is already in place for this).
- Optimistic UI updates on the frontend instead of waiting for each request to resolve.
- Pagination once task lists grow beyond a page.
- Editing a task's title (currently create/toggle/delete only).
- Promote `Core`/`Adapters` to separate class libraries if the backend grows.
- A real user store (registration, hashed passwords) and refresh tokens, per the trade-offs above.
- Per-user task ownership - right now all tasks are global/shared once you're signed in,
  since there's only ever one demo user.
