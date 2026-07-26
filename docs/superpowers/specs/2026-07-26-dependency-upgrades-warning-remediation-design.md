# Dependency Upgrades and Warning Remediation Design

## Context

The Central Package Management and SLNX migration preserved the repository's
existing package graph by design. The resulting Release build succeeds and all
24 tests pass, but restore/build emits six NuGet audit warnings representing
four vulnerable package/version pairs:

- `SQLitePCLRaw.lib.e_sqlite3` `2.1.11` through
  `Microsoft.EntityFrameworkCore.Sqlite`
- `Microsoft.OpenApi` `2.4.1` through `Swashbuckle.AspNetCore`
- `NuGet.Packaging` `6.12.1` through
  `Microsoft.VisualStudio.Web.CodeGeneration.Design`
- `NuGet.Protocol` `6.12.1` through
  `Microsoft.VisualStudio.Web.CodeGeneration.Design`

Rider's Problems view independently reports the same four vulnerable pairs.
The direct `xunit` `2.9.3` package is also deprecated and identifies
`xunit.v3` as its replacement.

The repository will remain on .NET 10. "Latest" means the latest stable
release compatible with `net10.0`; packages already on a prerelease track may
advance within that track. This excludes adopting .NET 11 previews, Mediator
`3.1.0-rc.1`, xUnit runner 4 previews, or any other new preview/RC track.

## Goals

- Update every direct dependency for which a newer compatible release exists.
- Keep every Microsoft .NET 10 package family on a consistent `10.0.10`
  patch level.
- Advance existing prerelease dependencies without introducing new prerelease
  dependency families.
- Remove unused package dependencies instead of retaining vulnerable
  tool-only graphs.
- Migrate the deprecated xUnit v2 package to stable xUnit v3.
- Resolve every current NuGet audit warning without blanket suppression.
- Preserve application behavior and keep all existing tests passing.
- Verify the final graph from both the .NET CLI and Rider.

## Non-Goals

- Migrating the repository to .NET 11 or adopting .NET 11 package previews.
- Upgrading stable dependencies to RC, preview, or nightly releases.
- Changing application architecture or performing unrelated refactors.
- Adding new runtime features or changing database schemas.
- Retaining optional ASP.NET scaffolding support that has no repository use.
- Suppressing an advisory merely because its vulnerable package is transitive.
- Modifying or committing the unrelated `.codex/` directory.

## Upgrade Policy

The first implementation stage will update all compatible direct dependencies
in `Directory.Packages.props`. Stable dependencies move to their latest stable
versions. Existing prerelease dependencies move only within their current
package track:

- `NetEscapades.EnumGenerators` moves from `1.0.0-beta08` to
  `1.0.0-beta21`.
- `Riok.Mapperly` moves from `5.0.0-next.3` to `5.0.0-next.10`.
- `Serilog.Expressions` remains at `5.1.0-dev-02309` because the configured
  package sources report no newer compatible release.

Packages with no compatible update remain unchanged. The deprecated `xunit`
package is handled as a separate migration because its replacement changes the
package ID and the test-project output model.

The upgrade will be applied in logical groups with restore, Release build, and
test checks between groups. If a package update introduces an API compilation
failure, implementation stops at that group so the exact compatibility change
can be diagnosed before proceeding.

## Warning Remediation

### Microsoft.OpenApi

`Swashbuckle.AspNetCore` will move from `10.1.7` to `10.2.3`.
`Swashbuckle.AspNetCore.Swagger` `10.2.3` requires
`Microsoft.OpenApi >= 2.7.5`, which is the patched 2.x floor for
GHSA-v5pm-xwqc-g5wc. No explicit OpenAPI pin is needed.

### SQLite

`Microsoft.EntityFrameworkCore.Sqlite` will move to `10.0.10`, but that
package still declares `SQLitePCLRaw.bundle_e_sqlite3 >= 2.1.11`. NuGet may
therefore continue selecting the vulnerable `2.1.11` native bundle.

`Directory.Packages.props` will enable
`CentralPackageTransitivePinningEnabled` and add a central
`PackageVersion` for `SQLitePCLRaw.bundle_e_sqlite3` `2.1.12`. This keeps the
override on the compatible 2.x line while selecting a bundle no longer
reported by NuGet audit as affected by GHSA-2m69-gcr7-jv3q.

### NuGet Client Libraries

`Microsoft.VisualStudio.Web.CodeGeneration.Design` is referenced only by
`MainServer/MainServer.csproj`. No source file, script, manifest, or active
repository documentation invokes ASP.NET scaffolding. The package is not
required for runtime behavior, EF migrations, build, or test execution.

The package reference and its central version will be removed. This eliminates
the unused scaffolding graph that introduces vulnerable `NuGet.Packaging`
`6.12.1` and `NuGet.Protocol` `6.12.1`; the repository will not add transitive
pins solely to preserve unused tooling.

### xUnit

The test project will replace `xunit` `2.9.3` with `xunit.v3` `3.2.2`, retain
the latest stable 3.x Visual Studio runner, and set
`<OutputType>Exe</OutputType>` as required by xUnit v3. The test code uses only
standard `[Fact]` tests and assertions, so no known source-level migration is
required. Test discovery and all 24 test outcomes must nevertheless be
verified through both `dotnet test` and Rider.

## Suppression Policy

No current warning will be suppressed. After upgrades and graph remediation,
any remaining warning must be traced to its direct dependency path and checked
against the advisory's affected versions and the repository's actual use.

Suppression is allowed only when all of the following are documented:

1. no patched compatible dependency path exists;
2. removing the dependency would break required repository behavior;
3. the vulnerable code path is demonstrably unreachable in this application;
4. the suppression targets the individual advisory rather than disabling
   NuGet audit globally.

If those conditions are not all met, the dependency graph must be fixed
instead.

## Verification

The pre-change Release build and NuGet audit output are the failing baseline.
The implementation is complete only when:

1. `dotnet restore GC-local-server-rewrite.slnx` succeeds without NuGet
   warnings.
2. `dotnet build GC-local-server-rewrite.slnx -c Release --no-restore`
   succeeds with zero warnings and zero errors.
3. `dotnet test GC-local-server-rewrite.slnx -c Release --no-build` discovers
   and passes all 24 tests.
4. `dotnet list GC-local-server-rewrite.slnx package --vulnerable
   --include-transitive` reports no vulnerable packages.
5. `dotnet list GC-local-server-rewrite.slnx package --deprecated` reports no
   deprecated direct packages.
6. A stable-only outdated-package query reports no remaining compatible direct
   updates, apart from packages intentionally retained by this design.
7. A prerelease-inclusive query confirms that excluded candidates belong only
   to unapproved .NET 11, RC, preview, or unrelated prerelease tracks.
8. Rider completes a full solution rebuild successfully and its Problems view
   reports no Security, NuGet, Solution, or Roslyn warnings/errors caused by
   the migration.

## Risks and Controls

- **Major-version compatibility:** update in logical groups and run the full
  Release build and test suite before committing each group.
- **Microsoft package skew:** update related ASP.NET Core, EF Core, and
  Microsoft.Extensions packages together at `10.0.10`.
- **Source-generator regressions:** rebuild all projects after advancing the
  existing enum-generator and Mapperly prerelease tracks.
- **Native SQLite mismatch:** pin the bundle rather than the leaf native
  library so its managed configuration and native library remain coordinated.
- **Test discovery drift:** require exactly 24 passing tests after the xUnit v3
  migration and verify discovery independently in Rider.
- **Unnecessary suppression:** treat suppression as an exceptional,
  advisory-specific last resort and record its applicability proof.
- **Scope contamination:** stage only dependency, test-project, design, and
  plan files; leave `.codex/` untouched.

## Success Criteria

All compatible dependencies are current under the approved .NET 10 policy,
the unused scaffolding package is removed, xUnit v3 runs the complete test
suite, the SQLite transitive dependency is safely pinned, CLI restore/build
output is warning-free, NuGet reports no vulnerable or deprecated packages,
and Rider reports a clean successful rebuild.
