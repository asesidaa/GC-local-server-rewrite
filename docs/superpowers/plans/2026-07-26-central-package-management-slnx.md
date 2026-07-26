# Central Package Management and SLNX Migration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Centralize every direct NuGet package version and replace the repository's legacy solutions with one authoritative root `.slnx` without changing dependencies or runtime behavior.

**Architecture:** A root `Directory.Packages.props` will own the 37 unique direct package versions while each consuming project retains a versionless `PackageReference` and its existing asset metadata. The .NET 10 SDK converter will generate the root `.slnx`; the two legacy `.sln` files will be removed, active automation and guidance will move to `.slnx`, and CLI plus Rider checks will verify the unchanged project and dependency graph.

**Tech Stack:** .NET SDK 10.0.201, NuGet Central Package Management, XML solution format (`.slnx`), PowerShell, GitHub Actions, Rider MCP

## Global Constraints

- Preserve every current direct NuGet package ID and version exactly.
- Preserve all existing `PrivateAssets`, `IncludeAssets`, and `ExcludeAssets` metadata.
- Do not add global package references, transitive pinning, dependency upgrades, target-framework changes, or application-code changes.
- Keep `GC-local-server-rewrite.slnx` as the sole solution file.
- Remove `WebUI/WebUI.sln`; WebUI remains directly buildable through `WebUI/WebUI.csproj`.
- Update only active references in `.github/workflows/build.yml`, `AGENTS.md`, and `CLAUDE.md`; do not rewrite completed historical plans or specs.
- Leave the unrelated untracked `.codex/` directory untouched and uncommitted.

---

## File Map

- Create `Directory.Packages.props`: the single version catalog for all direct NuGet dependencies.
- Modify `GCRelayServer/GCRelayServer.csproj`: remove inline versions from four package references.
- Modify `MainServer/MainServer.csproj`: remove inline versions from nine package references while retaining EF Tools metadata.
- Modify `Application/Application.csproj`: remove inline versions from seven package references while retaining source-generator metadata.
- Modify `Domain/Domain.csproj`: remove the enum-generator inline version while retaining analyzer asset metadata.
- Modify `Infrastructure/Infrastructure.csproj`: remove inline versions from ten package references while retaining EF Design metadata.
- Modify `WebUI/WebUI.csproj`: remove inline versions from five package references while retaining DevServer metadata.
- Modify `Tests/XmlSerializationTests/XmlSerializationTests.csproj`: remove inline versions from four test package references.
- Create `GC-local-server-rewrite.slnx`: generated authoritative solution with nine projects and the existing `Tests` solution folder.
- Delete `GC-local-server-rewrite.sln`: replaced by the generated root `.slnx`.
- Delete `WebUI/WebUI.sln`: unused single-project duplicate.
- Modify `.github/workflows/build.yml`: point restore, build, and test steps to the root `.slnx`.
- Modify `AGENTS.md`: point active solution descriptions and commands to the root `.slnx`.
- Modify `CLAUDE.md`: point the whole-solution build command to the root `.slnx`.

### Task 1: Centralize NuGet Package Versions

**Files:**
- Create: `Directory.Packages.props`
- Modify: `GCRelayServer/GCRelayServer.csproj:11-16`
- Modify: `MainServer/MainServer.csproj:15-28`
- Modify: `Application/Application.csproj:21-32`
- Modify: `Domain/Domain.csproj:10-14`
- Modify: `Infrastructure/Infrastructure.csproj:10-24`
- Modify: `WebUI/WebUI.csproj:10-16`
- Modify: `Tests/XmlSerializationTests/XmlSerializationTests.csproj:10-15`

**Interfaces:**
- Consumes: the 40 explicit `PackageReference` rows and their current versions from the seven project files.
- Produces: one root `Directory.Packages.props` containing 37 unique `PackageVersion` entries and versionless project-local `PackageReference` rows with unchanged asset metadata.

- [ ] **Step 1: Confirm the scoped starting state**

Run:

```powershell
git status --short
git log -2 --oneline
```

Expected: the design and plan commits are present; the only unrelated working-tree entry is `?? .codex/`. Stop if any tracked file in this task is already modified.

- [ ] **Step 2: Capture the normalized pre-migration package inventory**

Run:

```powershell
$solutionPath = 'GC-local-server-rewrite.sln'
$inventoryPath = Join-Path $env:TEMP 'gc-local-server-packages-before.json'
$rootPath = (Get-Location).Path
$packageJson = dotnet list $solutionPath package --include-transitive --format json --output-version 1 | Out-String | ConvertFrom-Json
$inventory = @(
    foreach ($project in $packageJson.projects) {
        $relativeProject = [IO.Path]::GetRelativePath($rootPath, [string]$project.path).Replace('\', '/')
        foreach ($framework in $project.frameworks) {
            foreach ($package in $framework.topLevelPackages) {
                [pscustomobject]@{
                    Project = $relativeProject
                    Framework = [string]$framework.framework
                    Kind = if ([string]$package.autoReferenced -eq 'true') { 'AutoReferenced' } else { 'Direct' }
                    Package = [string]$package.id
                    RequestedVersion = [string]$package.requestedVersion
                    ResolvedVersion = [string]$package.resolvedVersion
                }
            }
            foreach ($package in $framework.transitivePackages) {
                [pscustomobject]@{
                    Project = $relativeProject
                    Framework = [string]$framework.framework
                    Kind = 'Transitive'
                    Package = [string]$package.id
                    RequestedVersion = ''
                    ResolvedVersion = [string]$package.resolvedVersion
                }
            }
        }
    }
) | Sort-Object Project, Framework, Kind, Package
$inventory | ConvertTo-Json -Depth 4 | Set-Content -Encoding utf8 $inventoryPath
$directCount = @($inventory | Where-Object Kind -eq 'Direct').Count
if ($directCount -ne 40) { throw "Expected 40 direct package-reference rows, found $directCount." }
"$directCount direct package-reference rows and $($inventory.Count) total resolved rows written to $inventoryPath"
```

Expected: `40 direct package-reference rows` plus a larger total containing SDK-auto-referenced and transitive packages, written to a JSON file under the current user's temporary directory. Do not add this transient file to Git.

- [ ] **Step 3: Create the central package catalog**

Create `Directory.Packages.props` with exactly:

```xml
<Project>
    <PropertyGroup>
        <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    </PropertyGroup>

    <ItemGroup>
        <PackageVersion Include="BinarySerializer" Version="8.6.4.1" />
        <PackageVersion Include="CertificateManager" Version="1.0.9" />
        <PackageVersion Include="coverlet.collector" Version="6.0.4" />
        <PackageVersion Include="FlexLabs.EntityFrameworkCore.Upsert" Version="10.0.0" />
        <PackageVersion Include="Mediator.Abstractions" Version="3.0.2" />
        <PackageVersion Include="Mediator.SourceGenerator" Version="3.0.2" />
        <PackageVersion Include="Microsoft.AspNetCore.Components.WebAssembly" Version="10.0.5" />
        <PackageVersion Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="10.0.5" />
        <PackageVersion Include="Microsoft.AspNetCore.Components.WebAssembly.Server" Version="10.0.5" />
        <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="10.0.5" />
        <PackageVersion Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.5" />
        <PackageVersion Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />
        <PackageVersion Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.5" />
        <PackageVersion Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.5" />
        <PackageVersion Include="Microsoft.Extensions.Configuration" Version="10.0.5" />
        <PackageVersion Include="Microsoft.Extensions.Configuration.FileExtensions" Version="10.0.5" />
        <PackageVersion Include="Microsoft.Extensions.Configuration.Json" Version="10.0.5" />
        <PackageVersion Include="Microsoft.Extensions.DependencyInjection" Version="10.0.5" />
        <PackageVersion Include="Microsoft.Extensions.Http" Version="10.0.5" />
        <PackageVersion Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.5" />
        <PackageVersion Include="Microsoft.Extensions.Options" Version="10.0.5" />
        <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
        <PackageVersion Include="Microsoft.VisualStudio.Web.CodeGeneration.Design" Version="10.0.2" />
        <PackageVersion Include="MudBlazor" Version="9.2.0" />
        <PackageVersion Include="NetCoreServer" Version="8.0.7" />
        <PackageVersion Include="NetEscapades.EnumGenerators" Version="1.0.0-beta08" />
        <PackageVersion Include="Quartz.AspNetCore" Version="3.16.1" />
        <PackageVersion Include="Riok.Mapperly" Version="5.0.0-next.3" />
        <PackageVersion Include="Serilog" Version="4.2.0" />
        <PackageVersion Include="Serilog.AspNetCore" Version="10.0.0" />
        <PackageVersion Include="Serilog.Expressions" Version="5.1.0-dev-02309" />
        <PackageVersion Include="Serilog.Sinks.Console" Version="6.0.0" />
        <PackageVersion Include="Swashbuckle.AspNetCore" Version="10.1.7" />
        <PackageVersion Include="Throw" Version="1.4.0" />
        <PackageVersion Include="Validation" Version="2.6.68" />
        <PackageVersion Include="xunit" Version="2.9.3" />
        <PackageVersion Include="xunit.runner.visualstudio" Version="3.1.4" />
    </ItemGroup>
</Project>
```

- [ ] **Step 4: Make GCRelayServer consume central versions**

Replace the package item group in `GCRelayServer/GCRelayServer.csproj` with:

```xml
    <ItemGroup>
      <PackageReference Include="BinarySerializer" />
      <PackageReference Include="NetCoreServer" />
      <PackageReference Include="Serilog" />
      <PackageReference Include="Serilog.Sinks.Console" />
    </ItemGroup>
```

- [ ] **Step 5: Make MainServer consume central versions**

Replace the package item group in `MainServer/MainServer.csproj` with:

```xml
    <ItemGroup>
        <PackageReference Include="CertificateManager" />
        <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.Server" />
        <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" />
        <PackageReference Include="Microsoft.EntityFrameworkCore.Tools">
            <PrivateAssets>all</PrivateAssets>
            <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
        </PackageReference>
        <PackageReference Include="Microsoft.VisualStudio.Web.CodeGeneration.Design" />
        <PackageReference Include="Serilog.AspNetCore" />
        <PackageReference Include="Serilog.Expressions" />
        <PackageReference Include="Swashbuckle.AspNetCore" />
        <PackageReference Include="Throw" />
    </ItemGroup>
```

- [ ] **Step 6: Make Application consume central versions**

Replace the package item group in `Application/Application.csproj` with:

```xml
    <ItemGroup>
      <PackageReference Include="FlexLabs.EntityFrameworkCore.Upsert" />
      <PackageReference Include="Mediator.Abstractions" />
      <PackageReference Include="Mediator.SourceGenerator">
        <PrivateAssets>all</PrivateAssets>
        <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
      </PackageReference>
      <PackageReference Include="Microsoft.EntityFrameworkCore" />
      <PackageReference Include="Quartz.AspNetCore" />
      <PackageReference Include="Riok.Mapperly" />
      <PackageReference Include="Throw" />
    </ItemGroup>
```

- [ ] **Step 7: Make Domain consume the central enum-generator version**

Replace the package item group in `Domain/Domain.csproj` with:

```xml
    <ItemGroup>
      <PackageReference Include="NetEscapades.EnumGenerators" PrivateAssets="all" ExcludeAssets="runtime">
        <IncludeAssets>compile; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      </PackageReference>
    </ItemGroup>
```

- [ ] **Step 8: Make Infrastructure consume central versions**

Replace the package item group in `Infrastructure/Infrastructure.csproj` with:

```xml
    <ItemGroup>
      <PackageReference Include="CertificateManager" />
      <PackageReference Include="Microsoft.EntityFrameworkCore.Design">
        <PrivateAssets>all</PrivateAssets>
        <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      </PackageReference>
      <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" />
      <PackageReference Include="Microsoft.Extensions.Configuration" />
      <PackageReference Include="Microsoft.Extensions.Configuration.FileExtensions" />
      <PackageReference Include="Microsoft.Extensions.Configuration.Json" />
      <PackageReference Include="Microsoft.Extensions.DependencyInjection" />
      <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
      <PackageReference Include="Microsoft.Extensions.Options" />
      <PackageReference Include="Validation" />
    </ItemGroup>
```

- [ ] **Step 9: Make WebUI consume central versions**

Replace the package item group in `WebUI/WebUI.csproj` with:

```xml
    <ItemGroup>
        <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" />
        <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" PrivateAssets="all" />
        <PackageReference Include="Microsoft.Extensions.Http" />
        <PackageReference Include="MudBlazor" />
        <PackageReference Include="Throw" />
    </ItemGroup>
```

- [ ] **Step 10: Make XmlSerializationTests consume central versions**

Replace the package item group in `Tests/XmlSerializationTests/XmlSerializationTests.csproj` with:

```xml
  <ItemGroup>
    <PackageReference Include="coverlet.collector" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
  </ItemGroup>
```

- [ ] **Step 11: Verify the static central-package contract**

Run:

```powershell
rg -n --glob '*.csproj' '<PackageReference[^>]*\bVersion=' .
if ($LASTEXITCODE -eq 0) { throw 'Inline PackageReference versions remain.' }
if ($LASTEXITCODE -gt 1) { throw "rg failed with exit code $LASTEXITCODE." }

[xml]$centralXml = Get-Content -Raw 'Directory.Packages.props'
$centralIds = @($centralXml.Project.ItemGroup.PackageVersion | ForEach-Object { [string]$_.Include })
$projectIds = @(
    Get-ChildItem -Recurse -Filter '*.csproj' | ForEach-Object {
        [xml]$projectXml = Get-Content -Raw $_.FullName
        $projectXml.Project.ItemGroup.PackageReference |
            Where-Object { $null -ne $_.Include } |
            ForEach-Object { [string]$_.Include }
    }
)
$differences = Compare-Object ($projectIds | Sort-Object -Unique) ($centralIds | Sort-Object -Unique)
$duplicates = $centralIds | Group-Object | Where-Object Count -ne 1
if ($centralIds.Count -ne 37) { throw "Expected 37 central package IDs, found $($centralIds.Count)." }
if ($differences) { $differences | Format-Table | Out-String | Write-Host; throw 'Central and project package IDs differ.' }
if ($duplicates) { $duplicates | Format-Table | Out-String | Write-Host; throw 'Duplicate central package IDs found.' }
'Central package contract is complete and unique.'
```

Expected: no `rg` matches and `Central package contract is complete and unique.`

- [ ] **Step 12: Restore through the still-authoritative legacy solution**

Run:

```powershell
dotnet restore GC-local-server-rewrite.sln
```

Expected: exit code `0`, with no missing-central-version or duplicate-version errors.

- [ ] **Step 13: Capture and compare the post-CPM package inventory**

Run:

```powershell
$solutionPath = 'GC-local-server-rewrite.sln'
$inventoryPath = Join-Path $env:TEMP 'gc-local-server-packages-after-cpm.json'
$rootPath = (Get-Location).Path
$packageJson = dotnet list $solutionPath package --include-transitive --format json --output-version 1 | Out-String | ConvertFrom-Json
$inventory = @(
    foreach ($project in $packageJson.projects) {
        $relativeProject = [IO.Path]::GetRelativePath($rootPath, [string]$project.path).Replace('\', '/')
        foreach ($framework in $project.frameworks) {
            foreach ($package in $framework.topLevelPackages) {
                [pscustomobject]@{
                    Project = $relativeProject
                    Framework = [string]$framework.framework
                    Kind = if ([string]$package.autoReferenced -eq 'true') { 'AutoReferenced' } else { 'Direct' }
                    Package = [string]$package.id
                    RequestedVersion = [string]$package.requestedVersion
                    ResolvedVersion = [string]$package.resolvedVersion
                }
            }
            foreach ($package in $framework.transitivePackages) {
                [pscustomobject]@{
                    Project = $relativeProject
                    Framework = [string]$framework.framework
                    Kind = 'Transitive'
                    Package = [string]$package.id
                    RequestedVersion = ''
                    ResolvedVersion = [string]$package.resolvedVersion
                }
            }
        }
    }
) | Sort-Object Project, Framework, Kind, Package
$inventory | ConvertTo-Json -Depth 4 | Set-Content -Encoding utf8 $inventoryPath
$before = Get-Content (Join-Path $env:TEMP 'gc-local-server-packages-before.json')
$after = Get-Content $inventoryPath
$difference = Compare-Object $before $after
if ($difference) { $difference | Format-Table | Out-String | Write-Host; throw 'Resolved package inventory changed.' }
'Resolved direct, auto-referenced, and transitive package inventory is unchanged.'
```

Expected: `Resolved direct, auto-referenced, and transitive package inventory is unchanged.`

- [ ] **Step 14: Review and commit the Central Package Management migration**

Run:

```powershell
git diff --check
git diff -- Directory.Packages.props GCRelayServer/GCRelayServer.csproj MainServer/MainServer.csproj Application/Application.csproj Domain/Domain.csproj Infrastructure/Infrastructure.csproj WebUI/WebUI.csproj Tests/XmlSerializationTests/XmlSerializationTests.csproj
git add -- Directory.Packages.props GCRelayServer/GCRelayServer.csproj MainServer/MainServer.csproj Application/Application.csproj Domain/Domain.csproj Infrastructure/Infrastructure.csproj WebUI/WebUI.csproj Tests/XmlSerializationTests/XmlSerializationTests.csproj
git diff --cached --check
git diff --cached --stat
git diff --cached -- Directory.Packages.props GCRelayServer/GCRelayServer.csproj MainServer/MainServer.csproj Application/Application.csproj Domain/Domain.csproj Infrastructure/Infrastructure.csproj WebUI/WebUI.csproj Tests/XmlSerializationTests/XmlSerializationTests.csproj
git commit -m "Centralize NuGet package versions"
```

Expected: the staged diff contains only the new catalog and removal of inline `Version` attributes; the commit succeeds without staging `.codex/`.

### Task 2: Consolidate Solutions and Update Active Tooling

**Files:**
- Create: `GC-local-server-rewrite.slnx`
- Delete: `GC-local-server-rewrite.sln`
- Delete: `WebUI/WebUI.sln`
- Modify: `.github/workflows/build.yml:22-29`
- Modify: `AGENTS.md:5-18`
- Modify: `CLAUDE.md:11-14`

**Interfaces:**
- Consumes: the legacy root solution's nine project paths, three platforms, `Tests` solution folder, and the central package graph produced by Task 1.
- Produces: one authoritative root `.slnx`, active CI/documentation commands targeting it, and verified CLI and Rider builds.

- [ ] **Step 1: Capture the legacy solution project list**

Run:

```powershell
$legacyProjectsPath = Join-Path $env:TEMP 'gc-local-server-projects-before.txt'
$legacyProjects = @(
    dotnet sln GC-local-server-rewrite.sln list |
        Where-Object { $_ -match '\.csproj$' } |
        ForEach-Object { $_.Replace('\', '/') } |
        Sort-Object
)
if ($legacyProjects.Count -ne 9) { throw "Expected 9 projects, found $($legacyProjects.Count)." }
$legacyProjects | Set-Content -Encoding utf8 $legacyProjectsPath
$legacyProjects
```

Expected: nine project paths, including `WebUI/WebUI.csproj` and `Tests/XmlSerializationTests/XmlSerializationTests.csproj`.

- [ ] **Step 2: Generate the root SLNX with the .NET SDK converter**

Run:

```powershell
dotnet sln GC-local-server-rewrite.sln migrate
```

Expected: the CLI reports that `GC-local-server-rewrite.slnx` was generated.

- [ ] **Step 3: Verify the generated SLNX structure before deleting either legacy solution**

Run:

```powershell
$expected = @'
<Solution>
  <Configurations>
    <Platform Name="Any CPU" />
    <Platform Name="x64" />
    <Platform Name="x86" />
  </Configurations>
  <Folder Name="/Tests/">
    <Project Path="Tests/XmlSerializationTests/XmlSerializationTests.csproj" />
  </Folder>
  <Project Path="Application/Application.csproj" />
  <Project Path="DatParser/DatParser.csproj" />
  <Project Path="Domain/Domain.csproj" />
  <Project Path="GCRelayServer/GCRelayServer.csproj" />
  <Project Path="Infrastructure/Infrastructure.csproj" />
  <Project Path="MainServer/MainServer.csproj" />
  <Project Path="Shared/Shared.csproj" />
  <Project Path="WebUI/WebUI.csproj" />
</Solution>
'@
$actual = Get-Content -Raw 'GC-local-server-rewrite.slnx'
$normalizedExpected = $expected.Replace("`r`n", "`n").Trim()
$normalizedActual = $actual.Replace("`r`n", "`n").Trim()
if ($normalizedActual -ne $normalizedExpected) {
    throw 'Generated SLNX differs from the reviewed nine-project structure.'
}

$slnxProjects = @(
    dotnet sln GC-local-server-rewrite.slnx list |
        Where-Object { $_ -match '\.csproj$' } |
        ForEach-Object { $_.Replace('\', '/') } |
        Sort-Object
)
$legacyProjects = Get-Content (Join-Path $env:TEMP 'gc-local-server-projects-before.txt')
$difference = Compare-Object $legacyProjects $slnxProjects
if ($difference) { $difference | Format-Table | Out-String | Write-Host; throw 'SLNX project membership changed.' }
'SLNX structure and project membership match the legacy solution.'
```

Expected: `SLNX structure and project membership match the legacy solution.`

- [ ] **Step 4: Delete the two legacy solution files**

Use `apply_patch` with:

```text
*** Begin Patch
*** Delete File: GC-local-server-rewrite.sln
*** Delete File: WebUI/WebUI.sln
*** End Patch
```

Expected: the root `.sln` and unused WebUI-only `.sln` are deleted; the generated root `.slnx` remains.

- [ ] **Step 5: Point GitHub Actions at the root SLNX**

In `.github/workflows/build.yml`, make the three solution steps exactly:

```yaml
      - name: Restore dependencies
        run: dotnet restore GC-local-server-rewrite.slnx

      - name: Build solution
        run: dotnet build GC-local-server-rewrite.slnx -c Release --no-restore

      - name: Run tests
        run: dotnet test GC-local-server-rewrite.slnx -c Release --no-build
```

Do not alter action versions, publishing, artifact upload, or cleanup behavior.

- [ ] **Step 6: Update active repository guidance**

In `AGENTS.md`, use `GC-local-server-rewrite.slnx` in exactly these active statements:

```markdown
`GC-local-server-rewrite.slnx` targets .NET 10 and follows a layered design.

- `dotnet restore GC-local-server-rewrite.slnx` restores all projects.
- `dotnet build GC-local-server-rewrite.slnx -c Release` matches the CI build.
- `dotnet test GC-local-server-rewrite.slnx -c Release` runs the complete xUnit suite.

Run `dotnet format GC-local-server-rewrite.slnx` before submitting broad formatting changes.
```

Leave all other repository rules unchanged.

- [ ] **Step 7: Update the Claude whole-solution build command**

In `CLAUDE.md`, make the build entry exactly:

```bash
# Build entire solution
dotnet build GC-local-server-rewrite.slnx
```

Leave project-specific commands and architecture guidance unchanged.

- [ ] **Step 8: Verify the single-solution and active-reference contract**

Run:

```powershell
$solutions = @(rg --files -g '*.sln' -g '*.slnx')
if ($solutions.Count -ne 1 -or $solutions[0].Replace('\', '/') -ne 'GC-local-server-rewrite.slnx') {
    $solutions | Write-Host
    throw 'Expected GC-local-server-rewrite.slnx to be the sole solution.'
}

rg -n 'GC-local-server-rewrite\.sln\b|WebUI[/\\]WebUI\.sln\b' .github AGENTS.md CLAUDE.md
if ($LASTEXITCODE -eq 0) { throw 'An active legacy solution reference remains.' }
if ($LASTEXITCODE -gt 1) { throw "rg failed with exit code $LASTEXITCODE." }

$currentProjects = @(
    dotnet sln GC-local-server-rewrite.slnx list |
        Where-Object { $_ -match '\.csproj$' } |
        ForEach-Object { $_.Replace('\', '/') } |
        Sort-Object
)
$legacyProjects = Get-Content (Join-Path $env:TEMP 'gc-local-server-projects-before.txt')
$difference = Compare-Object $legacyProjects $currentProjects
if ($difference) { $difference | Format-Table | Out-String | Write-Host; throw 'Final project membership changed.' }
'Single-solution and active-reference checks passed.'
```

Expected: `Single-solution and active-reference checks passed.` Historical `.sln` command text under dated `docs/superpowers/` files is intentionally outside this active-reference check.

- [ ] **Step 9: Verify the package graph through the final SLNX**

Run:

```powershell
$solutionPath = 'GC-local-server-rewrite.slnx'
$inventoryPath = Join-Path $env:TEMP 'gc-local-server-packages-after-slnx.json'
$rootPath = (Get-Location).Path
$packageJson = dotnet list $solutionPath package --include-transitive --format json --output-version 1 | Out-String | ConvertFrom-Json
$inventory = @(
    foreach ($project in $packageJson.projects) {
        $relativeProject = [IO.Path]::GetRelativePath($rootPath, [string]$project.path).Replace('\', '/')
        foreach ($framework in $project.frameworks) {
            foreach ($package in $framework.topLevelPackages) {
                [pscustomobject]@{
                    Project = $relativeProject
                    Framework = [string]$framework.framework
                    Kind = if ([string]$package.autoReferenced -eq 'true') { 'AutoReferenced' } else { 'Direct' }
                    Package = [string]$package.id
                    RequestedVersion = [string]$package.requestedVersion
                    ResolvedVersion = [string]$package.resolvedVersion
                }
            }
            foreach ($package in $framework.transitivePackages) {
                [pscustomobject]@{
                    Project = $relativeProject
                    Framework = [string]$framework.framework
                    Kind = 'Transitive'
                    Package = [string]$package.id
                    RequestedVersion = ''
                    ResolvedVersion = [string]$package.resolvedVersion
                }
            }
        }
    }
) | Sort-Object Project, Framework, Kind, Package
$inventory | ConvertTo-Json -Depth 4 | Set-Content -Encoding utf8 $inventoryPath
$before = Get-Content (Join-Path $env:TEMP 'gc-local-server-packages-before.json')
$after = Get-Content $inventoryPath
$difference = Compare-Object $before $after
if ($difference) { $difference | Format-Table | Out-String | Write-Host; throw 'Final resolved package inventory changed.' }
'Final resolved direct, auto-referenced, and transitive package inventory is unchanged.'
```

Expected: `Final resolved direct, auto-referenced, and transitive package inventory is unchanged.`

- [ ] **Step 10: Run the Release CLI verification sequence**

Run:

```powershell
dotnet restore GC-local-server-rewrite.slnx
dotnet build GC-local-server-rewrite.slnx -c Release --no-restore
dotnet test GC-local-server-rewrite.slnx -c Release --no-build
```

Expected: restore and build exit with code `0`; the complete xUnit suite passes with no failed tests.

- [ ] **Step 11: Verify Rider's refreshed project and dependency model**

Call Rider MCP `get_solution_projects` with:

```json
{"rootFolder":"H:\\GC-local-server-rewrite"}
```

Expected: the returned modules include all nine project names: `Application`, `DatParser`, `Domain`, `GCRelayServer`, `Infrastructure`, `MainServer`, `Shared`, `WebUI`, and `XmlSerializationTests`. Rider may additionally expose the `Tests` solution folder and IDE miscellaneous modules.

Then call Rider MCP `get_project_dependencies` for each of these project names with the same `rootFolder`:

```text
Application
Infrastructure
MainServer
WebUI
```

Expected: Rider resolves the existing package and project dependencies for all four projects without a missing-package or unloaded-project error.

- [ ] **Step 12: Run an independent Rider solution build**

Call Rider MCP `build_solution_start` with:

```json
{"rootFolder":"H:\\GC-local-server-rewrite","rebuild":true}
```

Poll Rider MCP `build_solution_state` with `rootFolder` set to `H:\GC-local-server-rewrite` and `sessionId` set to the exact value returned by `build_solution_start`.

Expected terminal state: `Completed` with `buildIsSuccess: true`.

Finally call Rider MCP `get_project_problems` with:

```json
{"rootFolder":"H:\\GC-local-server-rewrite","severity":"Error"}
```

Expected: no current solution, NuGet, Roslyn, or build errors. If Rider remains attached to the removed `.sln`, open `GC-local-server-rewrite.slnx` in Rider, wait for project-model loading to finish, and repeat Steps 11-12 before accepting the Rider check.

- [ ] **Step 13: Review and commit the solution/tooling migration**

Run:

```powershell
git diff --check
git diff -- GC-local-server-rewrite.sln GC-local-server-rewrite.slnx WebUI/WebUI.sln .github/workflows/build.yml AGENTS.md CLAUDE.md
git add -A -- GC-local-server-rewrite.sln GC-local-server-rewrite.slnx WebUI/WebUI.sln .github/workflows/build.yml AGENTS.md CLAUDE.md
git diff --cached --check
git diff --cached --stat
git diff --cached -- GC-local-server-rewrite.sln GC-local-server-rewrite.slnx WebUI/WebUI.sln .github/workflows/build.yml AGENTS.md CLAUDE.md
git commit -m "Migrate repository tooling to SLNX"
```

Expected: the staged diff contains the generated root `.slnx`, both legacy solution deletions, and only the approved active-reference replacements; `.codex/` is not staged.

- [ ] **Step 14: Confirm the final repository state**

Run:

```powershell
git status --short
git log -4 --oneline
```

Expected: the package and solution migration commits are present, no tracked files remain modified, and `?? .codex/` is the only working-tree entry.
