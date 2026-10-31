# Stage 02 validation

Validation date: 2026-10-31.

## Scope

Stage 02 introduces the modular domain, ports and application orchestration.
Existing uncommitted Directory.Build.props settings were preserved; RouteEndpoint
and all its existing tests were moved from Routes to Routing without behavioral
changes. No runtime packages were added. The Adapters test project uses the same
pinned test packages as the existing test projects. Lockfiles were intentionally
regenerated for the new project graph.

## Checks

All mandatory local checks passed. Security configuration, warnings-as-errors,
NuGet Audit and analyzers remain enabled.

| Check | Result |
| --- | --- |
| Locked restore | Passed for all eight projects with NuGet Audit enabled |
| Release build | Passed, zero warnings and zero errors |
| xUnit tests | 69 passed: Domain 51, Application 16, architecture 2 |
| Formatting verification | Passed with no changes |
| git diff --check | Passed |
| NuGet vulnerability listing, including transitive packages | No vulnerable packages in any project |
| Semgrep OSS 1.182.1, repository rules | 34 production C# targets, two rules, zero findings |
| Trivy 0.76.1, repository configuration | Database update and filesystem scan passed; zero reported findings |

The existing Infrastructure test project intentionally has no tests because
Stage 02 introduces no concrete infrastructure services. It is not counted in
the 69 passing tests.

Semgrep's default test exclusions apply; all 34 production C# files were scanned.
Its two local rules are a limited check, not comprehensive security assurance.
Trivy inspected four NuGet lockfiles containing test dependencies. Production
projects contain no runtime packages. Trivy detected no supported infrastructure
configuration files; that is not evidence of approved infrastructure. Existing
HIGH/CRITICAL thresholds, secret scanning and exclusions were preserved.

## Environment findings

Restricted Windows execution initially blocked MSBuild parallel processes, the
formatter's named pipes, and intermittent terminal process creation. Serial
restore/build worked; the exact mandatory commands then all passed outside the
sandbox. An overlapping early build produced transient locked-file warnings;
after it exited, the final build completed with zero warnings and zero errors.
No project configuration was weakened to work around these restrictions.

The Windows PATH has no Semgrep or Trivy. Both pinned versions are installed in
Ubuntu-26.04 WSL and were executed there against this checkout. WSL requires
access outside the restricted sandbox.

GitHub CLI initially reported an invalid token inside the sandbox, but
authentication succeeded outside it. The token was not changed.

## Behavioral coverage

Existing endpoint tests were preserved. Additional tests cover numeric
boundaries, route/relay/game invariants, defensive executable metadata copying,
conservative traffic classification, measurement success/failure and
comparability, direct and relay selection, jitter/loss penalties, stable ties,
latest failed measurements, ambiguous observations, session transitions,
application orchestration, cancellation propagation and transport failures.
Architecture tests inspect project references for dependency direction and
cycles and verify functional source namespaces.

## Reproduction

```powershell
dotnet restore XboxLag.slnx --locked-mode
dotnet build XboxLag.slnx --configuration Release --no-restore
dotnet test XboxLag.slnx --configuration Release --no-restore
dotnet format XboxLag.slnx --verify-no-changes --no-restore
git diff --check
semgrep scan --config .semgrep/csharp.yml --error
trivy fs --config trivy.yaml .
```

If the environment cannot run parallel MSBuild, append -m:1 to restore, build and
test, and --disable-parallel to restore. Formatting and test runners may require
named-pipe/process access beyond a restricted sandbox.
