# Central Package Management and SLNX Migration Design

## Context

The repository targets .NET 10 and currently stores NuGet package versions directly in seven project files. Shared dependencies such as `CertificateManager`, `Throw`, and the Microsoft .NET 10 package family therefore repeat version information across projects.

The repository also contains two legacy solution files. `GC-local-server-rewrite.sln` is the authoritative root solution and includes every project. `WebUI/WebUI.sln` contains only `WebUI.csproj`, is not referenced by repository automation or documentation, and was created in the same commit that added WebUI to the root solution. It no longer provides a distinct build or development boundary.

The installed .NET 10 SDK supports NuGet Central Package Management and migration from `.sln` to the XML-based `.slnx` format. Rider recognizes the root solution's project and dependency graph and can provide an independent post-migration import and build check.

## Goals

- Manage every direct NuGet package version from one root `Directory.Packages.props`.
- Preserve the exact package versions and package-reference metadata used before the migration.
- Replace the authoritative root `.sln` with a generated `.slnx`.
- Remove the redundant standalone WebUI solution.
- Update active repository guidance and CI to use the new solution path.
- Prove that project membership, dependency resolution, build behavior, and tests remain unchanged.

## Non-Goals

- Upgrading, downgrading, adding, or removing NuGet packages.
- Enabling transitive pinning or adding repository-wide global package references.
- Centralizing target frameworks, language versions, or other project properties.
- Refactoring application code or changing runtime behavior.
- Rewriting dated, completed design and implementation documents that record commands used at the time.
- Retaining duplicate `.sln` and `.slnx` files for compatibility with older tooling.

## Central Package Management

A root `Directory.Packages.props` will set `ManagePackageVersionsCentrally` to `true` and declare one `PackageVersion` for every unique direct package used by the repository. Entries will be ordered by package ID to keep version changes easy to review.

Each affected `.csproj` will retain its existing `PackageReference` in the project where the dependency is consumed, but the `Version` attribute will be removed. Project-local metadata such as `PrivateAssets`, `IncludeAssets`, and `ExcludeAssets` will remain unchanged. Framework and project references are outside Central Package Management and will not be modified.

The migration will use the versions already declared in the project files. Dependency upgrades, vulnerability remediation, and preview-to-stable package changes require separate review.

## Solution Consolidation

The .NET SDK's `dotnet sln GC-local-server-rewrite.sln migrate` command will generate `GC-local-server-rewrite.slnx`. The generated solution must contain the same nine project files and retain the `Tests` solution-folder placement for `XmlSerializationTests`.

After the generated solution passes structural checks, the legacy root `.sln` will be removed. `WebUI/WebUI.sln` will also be removed rather than migrated: WebUI remains directly buildable through `WebUI/WebUI.csproj` and remains part of the root solution's complete dependency graph.

The active solution references in `.github/workflows/build.yml`, `AGENTS.md`, and `CLAUDE.md` will be changed to `GC-local-server-rewrite.slnx`. Completed documents under `docs/superpowers/` will remain unchanged as historical records.

## Verification

Before editing package declarations, the current direct and transitive package graph will be captured from the legacy solution. After migration:

1. `dotnet sln GC-local-server-rewrite.slnx list` must report the same nine project paths as the legacy solution.
2. Every project `PackageReference` must be versionless, and every direct package ID must have exactly one central `PackageVersion`.
3. The resolved direct package IDs and versions must match the pre-migration baseline.
4. `dotnet restore GC-local-server-rewrite.slnx` must succeed.
5. `dotnet build GC-local-server-rewrite.slnx -c Release --no-restore` must succeed.
6. `dotnet test GC-local-server-rewrite.slnx -c Release --no-build` must pass.
7. Rider must reopen or refresh the root solution, report the expected projects and dependencies, and complete a solution build without errors.

If restore reports a missing central version or a changed resolved package, the migration will stop and the central package inventory will be corrected before build or test results are accepted.

## Risks and Controls

- **Lost package metadata:** edit only the `Version` attribute and verify generator/design-time dependencies retain their asset controls.
- **Package graph drift:** compare the pre- and post-migration resolved direct-package inventory instead of relying only on a successful restore.
- **Solution membership drift:** use the SDK converter and compare project lists before deleting the legacy solution.
- **Redundant solution reintroduction:** document the root `.slnx` as the sole solution; retain direct `.csproj` commands for WebUI-only work.
- **Unrelated working-tree changes:** stage and commit only migration-owned files and leave the existing untracked `.codex/` directory untouched.

## Success Criteria

The migration is complete when all package versions are centralized without changing the resolved direct dependency set, `GC-local-server-rewrite.slnx` is the sole solution, active CI and repository guidance use it, the redundant WebUI solution is gone, CLI restore/build/tests pass, and Rider successfully imports and builds the migrated solution.
