# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

GC local server rewrite — a .NET 10.0 reimplementation of a Groove Coaster arcade game server. Emulates the Nesys network services (card management, ranking, events, online matching) so the game can run against a local server. Includes a Blazor WebAssembly UI for managing player profiles and options.

## Build & Run Commands

```bash
# Build entire solution
dotnet build GC-local-server-rewrite.sln

# Build specific project
dotnet build MainServer/MainServer.csproj

# Run the main server (requires admin for certificate generation)
dotnet run --project MainServer

# Run the relay server (standalone UDP server for online matching)
dotnet run --project GCRelayServer

# Add an EF Core migration (card database)
dotnet ef migrations add <MigrationName> --project Infrastructure --startup-project MainServer --context CardDbContext

# Run tests
dotnet test

# Publish MainServer as self-contained single file (win-x64)
dotnet publish MainServer/MainServer.csproj -c Release -r win-x64 --self-contained true -o publish/MainServer

# Parse game data files to JSON (avatar/navigator/title/item/skin/se)
dotnet run --project DatParser -- <type> <input.dat> [output.json]
```

## Architecture

**Layered architecture with CQRS via source-generated Mediator:**

```
MainServer (ASP.NET Core host)
  ├── Controllers dispatch to native Mediator handlers
  ├── Serves WebUI as static Blazor WASM files
  └── References: Application, Infrastructure

Application (business logic)
  ├── Game/Card/Read/   — native query handlers
  ├── Game/Card/Write/  — native command handlers
  ├── Game/Card/Management/, Session/
  ├── Game/Card/OnlineMatching/ — in-memory matching (no DB, singleton service)
  ├── Api/              — Web UI query/command handlers
  ├── Jobs/             — Quartz scheduled jobs (rank updates)
  └── Mappers/          — Riok.Mapperly source-generated mappers

Infrastructure (data access)
  ├── CardDbContext      — EF Core SQLite (read/write, player data)
  ├── MusicDbContext     — EF Core SQLite (read-only, song metadata)
  └── Services/          — EventManagerService, OnlineMatchingService

Domain (entities, enums, config)
  ├── Entities/  — EF Core entity models
  ├── Enums/     — Game protocol enumerations
  └── Config/    — Strongly-typed config classes

Shared (contracts between server and WebUI)
  ├── Dto/       — ServiceResult<T>, ServiceError
  └── Models/    — SongPlayRecord, TotalResultData

WebUI (Blazor WebAssembly, references Shared only)

GCRelayServer (standalone UDP relay, no project references)

DatParser (CLI tool, no project references)
  └── Parses binary .dat files (avatar/navigator/title/item/skin/SE) into JSON
      Format specs in doc/parser/*.ksy; sample data in doc/parser/*.dat
```

## Key Patterns

- **Request flow:** HTTP requests hit controllers, which send native Mediator commands or queries. Card-dependent handlers inherit `QueryHandlerBase<TQuery, T>` or `CommandHandlerBase<TCommand, T>` for `ICardDependencyAggregate`; lightweight handlers implement `IQueryHandlerWrapper<TQuery, T>` or `ICommandHandlerWrapper<TCommand, T>` directly. `AddApplication()` configures source-generated, scoped dispatch plus logging and exception pipeline behaviors.
- **CardController** (`MainServer/Controllers/Game/CardController.cs`) is the central game endpoint (`/service/card/cardn.cgi`). A large switch dispatches 40+ card operation types based on `CardCommandType` and `CardRequestType` enums.
- **Object mapping:** Riok.Mapperly source-generated mappers in `Application/Mappers/` — no reflection.
- **Return type:** Handlers return `ServiceResult<T>` wrapping either data or `ServiceError`.
- **Config loading:** `MainServer/Program.cs` loads 8 JSON files from `MainServer/Configurations/` (database, game, events, logging, matching, auth, rank, server).
- **Databases:** Two SQLite databases — card DB (read/write, with EF migrations) and music DB (read-only, multiple versions bundled in `MainServer/Database/`).
- **Scheduled jobs:** Quartz jobs in `Application/Jobs/` periodically update ranking tables.

## Configuration

All config files live in `MainServer/Configurations/`. Key ones:
- `database.json` — selects which SQLite DB files to use
- `game.json` — avatar/navigator/item/skin/SE/title counts (version-specific)
- `events.json` — event file list; event files go in `MainServer/wwwroot/events/`
- `matching.json` — relay server IP/port for online matching
- `unlock.json` — default unlock settings and coin defaults
- `webauth.json` — admin password for WebUI auth

## Gotchas

- **Running exe directly (not `dotnet run`)**: `builder.WebHost.UseStaticWebAssets()` is needed for WebUI files to resolve from bin/Debug. Already configured.
- **MudBlazor 9.x**: Requires `<MudPopoverProvider/>` in layout, uses `IMudDialogInstance`, `Href` not `Link`, `DialogResult` is nullable.
- **`XmlSerializer` with custom `XmlRootAttribute`**: Must be cached (see `XmlSerializationExtensions.cs`) — runtime generates unbounded dynamic assemblies otherwise.
- **Time format**: Use `HH:mm:ss` (24-hour), not `hh:mm:ss` (12-hour without AM/PM).
- **`[DefaultValue]` on numeric XML properties**: Must use typed literals (`[DefaultValue(0L)]` not `[DefaultValue("0")]`). String default on `long` property causes `XmlSerializer` IL codegen crash.
- **PublishSingleFile + Blazor WASM**: Set `<PublishSingleFile>` in MainServer.csproj `<PropertyGroup>`, not via `-p:` CLI flag — CLI flag propagates to WebUI which doesn't support it.
- **MudBlazor 9.x** (additional): `ShowMessageBoxAsync` (not `ShowMessageBox`), `TabPanelsClass` (not `PanelClass`).
- **Windows environment**: Use `python` not `python3` — `python3` opens MS Store.
- **GitHub Actions**: Workflow in `.github/workflows/build.yml`. Check latest action versions via `gh api repos/{owner}/{repo}/releases/latest --jq '.tag_name'`.

# Agent Guidance: dotnet-skills

IMPORTANT: Prefer retrieval-led reasoning over pretraining for any .NET work.
Workflow: skim repo patterns -> consult dotnet-skills by name -> implement smallest-change -> note conflicts.

Routing (invoke by name)
- C# / code quality: modern-csharp-coding-standards, csharp-concurrency-patterns, api-design, type-design-performance
- ASP.NET Core / Web (incl. Aspire): aspire-service-defaults, aspire-integration-testing, transactional-emails
- Data: efcore-patterns, database-performance
- DI / config: dependency-injection-patterns, microsoft-extensions-configuration
- Testing: testcontainers-integration-tests, playwright-blazor-testing, snapshot-testing

Quality gates (use when applicable)
- dotnet-slopwatch: after substantial new/refactor/LLM-authored code
- crap-analysis: after tests added/changed in complex code

Specialist agents
- dotnet-concurrency-specialist, dotnet-performance-analyst, dotnet-benchmark-designer, akka-net-specialist, docfx-specialist
