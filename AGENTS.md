# Repository Guidelines

## Project Structure & Module Organization

`GC-local-server-rewrite.slnx` targets .NET 10 and follows a layered design. `MainServer/` is the ASP.NET Core host and controller layer; `Application/` contains native Mediator commands, queries, jobs, and Mapperly mappers; `Domain/` owns entities, enums, and configuration models; and `Infrastructure/` provides EF Core persistence and external services. Shared server/WebUI contracts live in `Shared/`, while `WebUI/` contains the Blazor WebAssembly client. `GCRelayServer/` is the standalone matching relay, and `DatParser/` converts game `.dat` files to JSON. Put automated tests under `Tests/`; supporting format notes and parser samples belong in `doc/`. Runtime configuration and static event assets live under `MainServer/Configurations/` and `MainServer/wwwroot/`.

## Build, Test, and Development Commands

- `dotnet restore GC-local-server-rewrite.slnx` restores all projects.
- `dotnet build GC-local-server-rewrite.slnx -c Release` matches the CI build.
- `dotnet test GC-local-server-rewrite.slnx -c Release` runs the complete xUnit suite.
- `dotnet run --project MainServer/MainServer.csproj` starts the API and hosted WebUI; certificate generation may require Administrator privileges on Windows.
- `dotnet run --project GCRelayServer/GCRelayServer.csproj` starts online-matching relay services.
- `dotnet run --project DatParser/DatParser.csproj -- <type> <input.dat> [output.json]` converts supported game data.

## Coding Style & Naming Conventions

Use four-space indentation and standard C# formatting. Nullable reference types and implicit usings are enabled; preserve null-safety instead of suppressing warnings. Use `PascalCase` for types, methods, properties, and public members; `camelCase` for locals and parameters; and prefix interfaces with `I`. Name native Mediator messages by intent, such as `GetMusicListQuery` or `SetPlayerNameCommand`, and implement the matching query or command handler contract. Respect layer boundaries and use existing Mapperly mappers for object conversion. Run `dotnet format GC-local-server-rewrite.slnx` before submitting broad formatting changes.

## Testing Guidelines

Tests use xUnit, currently under `Tests/XmlSerializationTests/`. Name tests after behavior and condition, for example `SerializeCardData_WithXPath_ProducesCorrectStructure`, and use `[Fact]` for single scenarios. Add focused regression coverage for changed protocol serialization, handlers, or persistence behavior. No coverage threshold is configured; prioritize meaningful assertions over raw percentage.

## Commit & Pull Request Guidelines

Recent commits use short, imperative summaries beginning with verbs such as `Add`, `Fix`, `Update`, or `Migrate`. Keep each commit focused. Pull requests should explain behavior and configuration/database impacts, link relevant issues, list verification commands, and include screenshots for visible WebUI changes. Do not commit build output from `bin/`, `obj/`, or `publish/`, or real passwords and private certificates.
