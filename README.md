# BTP — Boomkwekerij Taak Planner

**Overview**
- Multi-project .NET 8 solution with `ASP.NET Core` API and UI, plus a Blazor WebAssembly Mobile UI.
- Data via `Entity Framework Core` targeting SQL Server.
- Containerized with `docker compose` and reverse-proxied by `Traefik`.
- CI/CD via GitLab: test, build Docker images, deploy with compose.

**Projects**
- `BTP/AP.BTP.API`: REST API (Swagger enabled in dev)
- `BTP/AP.BTP.UI`: Server-rendered web UI
- `BTP/AP.BTP.MobileUI`: Blazor WASM served by `nginx`
- `BTP/AP.BTP.Application`, `BTP/AP.BTP.Domain`, `BTP/AP.BTP.Infrastructure`: core logic and data access
- `BTP/AP.BTP.UnitTests`: automated tests

**Prerequisites**
- `.NET SDK 8`
- `Docker` and `Docker Compose`
- Optional for auth: `Auth0` tenant and app credentials

**Configuration**
- Prefer environment variables over committing secrets:
  - `ASPNETCORE_ENVIRONMENT` (e.g. `Development`, `Production`)
  - `ASPNETCORE_URLS` (e.g. `http://+:8081` for API, `http://+:8080` for UI)
  - `ConnectionStrings__BTPdb` (SQL Server)
  - `Auth0__Domain`, `Auth0__ClientId`, `Auth0__ClientSecret`, `Auth0__Audience`
  - UI/Mobile: `Api__BaseUrl`, `Api__PublicBaseUrl`
- Dev defaults live in `appsettings.*.json`; override in prod with env vars.

**Run Locally (no Docker)**
- API
  - `cd BTP/AP.BTP.API`
  - `dotnet run --project AP.BTP.API.csproj`
  - Dev URL: `http://localhost:5107` (Swagger at `/swagger`)
- Web UI
  - `cd BTP/AP.BTP.UI`
  - `dotnet run --project AP.BTP.UI.csproj`
  - Dev URL: `http://localhost:5087`
  - Ensure `Api.BaseUrl` points to the API dev URL
- Mobile UI (Blazor WASM)
  - `cd BTP/AP.BTP.MobileUI`
  - `dotnet run --project AP.BTP.MobileUI.csproj`
  - Dev URL: `http://localhost:5246`
  - `wwwroot/appsettings.Development.json` should reference the API dev URL
- Tests
  - From repo root: `dotnet test BTP/AP.BTP.UnitTests/AP.BTP.UnitTests.csproj`
  - Or per project as needed

**Run with Docker (recommended)**
- Create reverse proxy network:
  - `docker network create traefik`
- Start Traefik:
  - `cd traefik`
  - `docker compose up -d`
- Prepare `.env` at repo root for application compose:
  - Example:
    - `ASPNETCORE_ENVIRONMENT=Production`
    - `ASPNETCORE_URLS=http://+:8080`
    - `ACCEPT_EULA=Y`
    - `SA_PASSWORD=ChangeMeStrongPwd!`
    - `MSSQL_PID=Developer`
    - `ConnectionStrings__BTPdb=Server=sqlserver;Database=BTPdb;User Id=sa;Password=ChangeMeStrongPwd!;TrustServerCertificate=True;`
    - `Auth0__Domain=your-tenant.eu.auth0.com`
    - `Auth0__ClientId=...`
    - `Auth0__ClientSecret=...`
    - `Auth0__Audience=btp-api`
- Start the stack:
  - `docker compose pull`
  - `docker compose up -d`
- Access
  - API: `http://localhost:5000/swagger` (mapped `5000->8081`)
  - UI and Mobile: routed by Traefik based on host and path rules; adjust Traefik labels or your hosts file as needed.

**GitLab Setup**
- Create a GitLab project and push:
  - `git remote add origin https://gitlab.apstudent.be/bachelor-it/applied-software-project/25-26/team-06/asp-btp.git`
  - `git branch -M main && git push -u origin main`
- Enable Container Registry on the project.
- CI variables (Settings → CI/CD → Variables):
  - `CI_REGISTRY`, `CI_REGISTRY_USER`, `CI_REGISTRY_PASSWORD`
  - `DOT_ENV_FILE` containing the `.env` content for deployment target
  - Optional: Auth0 variables if not in `.env`
- Runner requirements:
  - Docker executor capable of building images and using `docker compose` on the deploy target via `DOCKER_HOST` or SSH with scripts provided in pipeline.

**CI/CD Pipeline**
- File: `.gitlab-cicd.yml`
- Stages:
  - `test`: runs `dotnet test` on API, UI, and core projects using `.NET SDK 8`
  - `build`: builds and pushes images for API (`btp-api`), Web UI (`btp-ui`), Mobile UI (`btp-mobile`) to the GitLab registry
  - `deploy` (main branch only): uploads compose and Traefik configs to `/srv/asp-btp`, ensures Traefik is up, recreates the stack with `.env` from `DOT_ENV_FILE`
- Deployment host prerequisites:
  - Docker, Docker Compose installed
  - External network `traefik` present
  - Write access to `/srv/asp-btp`

**Useful Paths**
- Solution: `BTP/BTP.sln`
- Dockerfiles: `BTP/AP.BTP.API/Dockerfile`, `BTP/AP.BTP.UI/Dockerfile`, `BTP/AP.BTP.MobileUI/Dockerfile`
- Compose: root `docker-compose.yml`, proxy `traefik/docker-compose.yml`, `traefik/traefik.yml`
- Launch settings: `BTP/*/Properties/launchSettings.json`

**Troubleshooting**
- API unreachable from UI/Mobile:
  - Check `Api__BaseUrl` and Traefik routing labels.
  - Verify `ASPNETCORE_URLS` and port mappings.
- SQL Server container fails to start:
  - Ensure `SA_PASSWORD` meets complexity and `ACCEPT_EULA=Y`.
- Auth errors:
  - Confirm `Auth0` domain, client credentials, and audience match API configuration.
