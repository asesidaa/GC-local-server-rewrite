# Dependency Upgrades and Warning Remediation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Update every compatible dependency in the .NET 10 solution, remove unused vulnerable tooling, migrate tests to xUnit v3, and finish with warning-free CLI and Rider builds.

**Architecture:** `Directory.Packages.props` remains the single dependency catalog. Direct packages are upgraded first so the post-upgrade graph can be observed before remediation; the remaining graph is then corrected by removing unused ASP.NET scaffolding, centrally pinning the SQLite bundle, and migrating the deprecated test framework package.

**Tech Stack:** .NET SDK 10.0.201, NuGet Central Package Management, .NET 10, xUnit v3, PowerShell, Rider MCP

## Global Constraints

- Remain on `net10.0`; do not adopt .NET 11 packages.
- Use the latest stable compatible release for stable dependencies.
- Advance only existing prerelease tracks: `NetEscapades.EnumGenerators` and `Riok.Mapperly`.
- Do not adopt Mediator RC, xUnit 4 preview, .NET 11 preview, or other new prerelease tracks.
- Keep all Microsoft .NET 10 package families aligned at `10.0.10`.
- Remove `Microsoft.VisualStudio.Web.CodeGeneration.Design`; do not pin its unused dependency graph.
- Pin `SQLitePCLRaw.bundle_e_sqlite3` to `2.1.12` through Central Package Management.
- Replace `xunit` `2.9.3` with `xunit.v3` `3.2.2`.
- Do not suppress any current advisory.
- Execute sequentially in the current branch; do not use agents or worktrees.
- Leave the unrelated untracked `.codex/` directory untouched and uncommitted.

---

## File Map

- Modify `Directory.Packages.props`: update compatible versions, remove the
  unused scaffolding and deprecated xUnit package IDs, enable transitive
  pinning, and add the SQLite bundle and xUnit v3 versions.
- Modify `MainServer/MainServer.csproj`: remove the unused ASP.NET Web Code
  Generation package reference.
- Modify `Tests/XmlSerializationTests/XmlSerializationTests.csproj`: replace
  the xUnit v2 package reference and make the xUnit v3 test assembly
  executable.

### Task 1: Upgrade All Compatible Direct Dependencies

**Files:**
- Modify: `Directory.Packages.props`

**Interfaces:**
- Consumes: the 37-package central catalog and the approved compatible-latest
  policy.
- Produces: the same direct package IDs at current compatible versions, while
  intentionally retaining `xunit` and Web Code Generation until their
  remaining warnings are analyzed in later tasks.

- [ ] **Step 1: Confirm the clean scoped starting state**

Run:

```powershell
git status --short
git log -5 --oneline
```

Expected: the design and implementation-plan commits follow the completed CPM
and SLNX commits; `?? .codex/` is the only unrelated working-tree entry.

- [ ] **Step 2: Reproduce and record the RED warning baseline**

Run:

```powershell
dotnet restore GC-local-server-rewrite.slnx
dotnet build GC-local-server-rewrite.slnx -c Release --no-restore
dotnet test GC-local-server-rewrite.slnx -c Release --no-build
dotnet list GC-local-server-rewrite.slnx package --vulnerable --include-transitive
dotnet list GC-local-server-rewrite.slnx package --deprecated
```

Expected: restore/build succeeds with six NuGet audit warning occurrences,
24 tests pass, the vulnerable report contains
`SQLitePCLRaw.lib.e_sqlite3 2.1.11`, `Microsoft.OpenApi 2.4.1`,
`NuGet.Packaging 6.12.1`, and `NuGet.Protocol 6.12.1`, and the deprecated
report contains `xunit 2.9.3`.

- [ ] **Step 3: Update the approved direct package versions**

In `Directory.Packages.props`, make exactly these version replacements:

| Package | From | To |
|---|---:|---:|
| `BinarySerializer` | `8.6.4.1` | `10.1.0` |
| `CertificateManager` | `1.0.9` | `1.0.10` |
| `coverlet.collector` | `6.0.4` | `10.0.1` |
| `Microsoft.AspNetCore.Components.WebAssembly` | `10.0.5` | `10.0.10` |
| `Microsoft.AspNetCore.Components.WebAssembly.DevServer` | `10.0.5` | `10.0.10` |
| `Microsoft.AspNetCore.Components.WebAssembly.Server` | `10.0.5` | `10.0.10` |
| `Microsoft.EntityFrameworkCore` | `10.0.5` | `10.0.10` |
| `Microsoft.EntityFrameworkCore.Design` | `10.0.5` | `10.0.10` |
| `Microsoft.EntityFrameworkCore.Sqlite` | `10.0.5` | `10.0.10` |
| `Microsoft.EntityFrameworkCore.SqlServer` | `10.0.5` | `10.0.10` |
| `Microsoft.EntityFrameworkCore.Tools` | `10.0.5` | `10.0.10` |
| `Microsoft.Extensions.Configuration` | `10.0.5` | `10.0.10` |
| `Microsoft.Extensions.Configuration.FileExtensions` | `10.0.5` | `10.0.10` |
| `Microsoft.Extensions.Configuration.Json` | `10.0.5` | `10.0.10` |
| `Microsoft.Extensions.DependencyInjection` | `10.0.5` | `10.0.10` |
| `Microsoft.Extensions.Http` | `10.0.5` | `10.0.10` |
| `Microsoft.Extensions.Logging.Abstractions` | `10.0.5` | `10.0.10` |
| `Microsoft.Extensions.Options` | `10.0.5` | `10.0.10` |
| `Microsoft.NET.Test.Sdk` | `17.14.1` | `18.8.1` |
| `MudBlazor` | `9.2.0` | `9.7.0` |
| `NetEscapades.EnumGenerators` | `1.0.0-beta08` | `1.0.0-beta21` |
| `Quartz.AspNetCore` | `3.16.1` | `3.19.0` |
| `Riok.Mapperly` | `5.0.0-next.3` | `5.0.0-next.10` |
| `Serilog` | `4.2.0` | `4.4.0` |
| `Serilog.Sinks.Console` | `6.0.0` | `6.1.1` |
| `Swashbuckle.AspNetCore` | `10.1.7` | `10.2.3` |
| `xunit.runner.visualstudio` | `3.1.4` | `3.1.5` |

Leave every other version unchanged, including Mediator `3.0.2`,
`Serilog.Expressions` `5.1.0-dev-02309`, and `xunit` `2.9.3`.

- [ ] **Step 4: Restore and verify the direct-upgrade group**

Run:

```powershell
dotnet restore GC-local-server-rewrite.slnx
dotnet build GC-local-server-rewrite.slnx -c Release --no-restore
dotnet test GC-local-server-rewrite.slnx -c Release --no-build
dotnet list GC-local-server-rewrite.slnx package --vulnerable --include-transitive
dotnet list GC-local-server-rewrite.slnx package --deprecated
```

Expected: restore/build succeeds, all 24 tests pass, the OpenAPI advisory is
gone, the SQLite and NuGet client warnings remain for Task 2, and `xunit`
remains the sole deprecated direct package for Task 3.

If the build fails, do not continue to Task 2. Trace the first failure to the
package group that owns it and make only the compatibility edit required to
restore the pre-upgrade behavior, with a focused failing regression test when
runtime behavior is involved.

- [ ] **Step 5: Confirm approved upgrade-channel boundaries**

Run:

```powershell
dotnet list GC-local-server-rewrite.slnx package --outdated
dotnet list GC-local-server-rewrite.slnx package --outdated --include-prerelease
```

Expected: the stable query has no actionable compatible update
(`NetEscapades.EnumGenerators` may be shown as unavailable on the stable-only
channel). The prerelease query shows only excluded RC/preview families such as
Mediator `3.1.0-rc.1`, .NET 11 previews, and xUnit runner 4 preview.

- [ ] **Step 6: Review and commit the direct dependency upgrades**

Run:

```powershell
git diff --check
git diff -- Directory.Packages.props
git add -- Directory.Packages.props
git diff --cached --check
git diff --cached -- Directory.Packages.props
git commit -m "Update compatible NuGet dependencies"
```

Expected: the commit contains only the 27 approved central version
replacements and does not stage `.codex/`.

### Task 2: Remove Unused Scaffolding and Fix Transitive Vulnerabilities

**Files:**
- Modify: `Directory.Packages.props`
- Modify: `MainServer/MainServer.csproj`

**Interfaces:**
- Consumes: the upgraded graph from Task 1 and its remaining SQLite and NuGet
  client audit warnings.
- Produces: a warning-free runtime/build dependency graph with one centrally
  pinned SQLite bundle and no ASP.NET scaffolding dependency.

- [ ] **Step 1: Enable central transitive pinning**

In `Directory.Packages.props`, make the root property group exactly:

```xml
    <PropertyGroup>
        <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
        <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
    </PropertyGroup>
```

- [ ] **Step 2: Replace the unused scaffolding version with the SQLite pin**

Remove this central entry:

```xml
        <PackageVersion Include="Microsoft.VisualStudio.Web.CodeGeneration.Design" Version="10.0.2" />
```

Add this entry in package-ID order between the Serilog and Swashbuckle
entries:

```xml
        <PackageVersion Include="SQLitePCLRaw.bundle_e_sqlite3" Version="2.1.12" />
```

- [ ] **Step 3: Remove the MainServer scaffolding reference**

Delete this line from `MainServer/MainServer.csproj`:

```xml
        <PackageReference Include="Microsoft.VisualStudio.Web.CodeGeneration.Design" />
```

Do not change EF Tools/Design references; they continue to support EF
migrations independently of ASP.NET controller/view scaffolding.

- [ ] **Step 4: Restore and verify the resolved security package versions**

Run:

```powershell
dotnet restore GC-local-server-rewrite.slnx
dotnet list GC-local-server-rewrite.slnx package --include-transitive
dotnet list GC-local-server-rewrite.slnx package --vulnerable --include-transitive
```

Expected: the resolved graph contains
`SQLitePCLRaw.bundle_e_sqlite3 2.1.12`, contains patched
`Microsoft.OpenApi 2.7.5` or newer, no longer contains the Web Code Generation
dependency chain, and reports no vulnerable packages.

- [ ] **Step 5: Verify build and behavior after graph cleanup**

Run:

```powershell
dotnet build GC-local-server-rewrite.slnx -c Release --no-restore
dotnet test GC-local-server-rewrite.slnx -c Release --no-build
```

Expected: build succeeds with zero warnings and zero errors; all 24 tests pass.

- [ ] **Step 6: Review and commit the transitive remediation**

Run:

```powershell
git diff --check
git diff -- Directory.Packages.props MainServer/MainServer.csproj
git add -- Directory.Packages.props MainServer/MainServer.csproj
git diff --cached --check
git diff --cached -- Directory.Packages.props MainServer/MainServer.csproj
git commit -m "Remediate transitive dependency vulnerabilities"
```

Expected: the commit enables central transitive pinning, adds the one SQLite
bundle pin, and removes only the unused scaffolding package.

### Task 3: Migrate the Test Project to xUnit v3

**Files:**
- Modify: `Directory.Packages.props`
- Modify: `Tests/XmlSerializationTests/XmlSerializationTests.csproj`

**Interfaces:**
- Consumes: 24 passing xUnit v2 tests and the deprecated `xunit` package
  warning.
- Produces: an executable xUnit v3 test project that retains Visual Studio and
  Rider discovery and passes the same 24 tests.

- [ ] **Step 1: Replace the central xUnit package**

In `Directory.Packages.props`, replace:

```xml
        <PackageVersion Include="xunit" Version="2.9.3" />
```

with:

```xml
        <PackageVersion Include="xunit.v3" Version="3.2.2" />
```

Keep `xunit.runner.visualstudio` at stable `3.1.5`.

- [ ] **Step 2: Convert the test project to the xUnit v3 output model**

Add this property to the existing property group in
`Tests/XmlSerializationTests/XmlSerializationTests.csproj`:

```xml
    <OutputType>Exe</OutputType>
```

In the package item group, replace:

```xml
    <PackageReference Include="xunit" />
```

with:

```xml
    <PackageReference Include="xunit.v3" />
```

Leave the global `<Using Include="Xunit" />` and all test source files
unchanged unless compilation proves an actual v3 incompatibility.

- [ ] **Step 3: Restore, build, and run the migrated test project**

Run:

```powershell
dotnet restore GC-local-server-rewrite.slnx
dotnet build GC-local-server-rewrite.slnx -c Release --no-restore
dotnet test Tests/XmlSerializationTests/XmlSerializationTests.csproj -c Release --no-build
dotnet test GC-local-server-rewrite.slnx -c Release --no-build
```

Expected: build has zero warnings/errors; both test commands discover exactly
24 tests and report 24 passed, 0 failed, 0 skipped.

- [ ] **Step 4: Prove the deprecated package is gone**

Run:

```powershell
dotnet list GC-local-server-rewrite.slnx package --deprecated
```

Expected: no deprecated packages are reported.

- [ ] **Step 5: Review and commit the xUnit v3 migration**

Run:

```powershell
git diff --check
git diff -- Directory.Packages.props Tests/XmlSerializationTests/XmlSerializationTests.csproj
git add -- Directory.Packages.props Tests/XmlSerializationTests/XmlSerializationTests.csproj
git diff --cached --check
git diff --cached -- Directory.Packages.props Tests/XmlSerializationTests/XmlSerializationTests.csproj
git commit -m "Migrate tests to xUnit v3"
```

Expected: the commit contains only the package-ID replacement and executable
test-project setting.

### Task 4: Run Final CLI and Rider Verification

**Files:**
- No source-file changes expected.

**Interfaces:**
- Consumes: the three dependency commits from Tasks 1-3.
- Produces: current CLI and IDE evidence that the approved graph is clean,
  compatible, and behaviorally unchanged.

- [ ] **Step 1: Run the final CLI verification sequence**

Run:

```powershell
dotnet restore GC-local-server-rewrite.slnx
dotnet build GC-local-server-rewrite.slnx -c Release --no-restore
dotnet test GC-local-server-rewrite.slnx -c Release --no-build
dotnet list GC-local-server-rewrite.slnx package --vulnerable --include-transitive
dotnet list GC-local-server-rewrite.slnx package --deprecated
dotnet list GC-local-server-rewrite.slnx package --outdated
dotnet list GC-local-server-rewrite.slnx package --outdated --include-prerelease
```

Expected:

- restore succeeds without warnings;
- Release build reports `0 Warning(s)` and `0 Error(s)`;
- exactly 24 tests pass;
- vulnerable and deprecated reports are empty;
- stable outdated output contains no actionable compatible update;
- prerelease-only candidates are limited to the explicitly excluded
  .NET 11, RC, preview, or new prerelease tracks.

- [ ] **Step 2: Verify Rider's loaded solution and run a full rebuild**

Call Rider MCP `get_solution_projects` with:

```json
{"rootFolder":"H:\\GC-local-server-rewrite"}
```

Expected: all nine repository projects remain loaded.

Call Rider MCP `build_solution_start` with:

```json
{"rootFolder":"H:\\GC-local-server-rewrite","rebuild":true}
```

Poll Rider MCP `build_solution_state` with the returned `sessionId` until it
reports `Completed`.

Expected: `buildIsSuccess` is `true`.

- [ ] **Step 3: Inspect Rider problems by subsystem**

Call Rider MCP `get_project_problems` for `Security`, `NuGet`, `Solution`, and
`Roslyn`, always with:

```json
{"rootFolder":"H:\\GC-local-server-rewrite","severity":"Warning","subsystem":"SUBSYSTEM"}
```

Replace `SUBSYSTEM` for each call.

Expected: no warning or error remains in any of the four subsystems.

- [ ] **Step 4: Confirm the final repository state**

Run:

```powershell
git diff --check
git status --short
git log -8 --oneline
```

Expected: no tracked file is modified, `.codex/` is still the only untracked
entry, and the design, plan, direct-upgrade, remediation, and xUnit migration
commits are present.
