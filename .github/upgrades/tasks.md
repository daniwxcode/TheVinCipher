# Upgrade Tasks: .NET 6.0 ? .NET 10.0 — Solution HermesEyes

## Progress Dashboard

| Status | Count |
|:---|:---:|
| ? Completed | 0 |
| ?? In Progress | 0 |
| ? Not Started | 7 |
| ? Failed | 0 |
| ? Skipped | 0 |
| **Total** | **7** |

---

## Tasks

### [?] TASK-001: Verify Prerequisites
**Scope**: Solution-wide
**References**: Plan §2 Phase 0

**Actions:**
- [?] (1) Verify .NET 10 SDK is installed on the machine
- [ ] (2) Validate `global.json` compatibility with .NET 10.0 (if present)

**Verification**: .NET 10 SDK available; no global.json conflicts

---

### [ ] TASK-002: Update Target Frameworks to net10.0
**Scope**: All 4 projects
**References**: Plan §4.1–4.4

**Actions:**
- [ ] (1) Update `<TargetFramework>net6.0</TargetFramework>` ? `<TargetFramework>net10.0</TargetFramework>` in `Domaine\Domaine.csproj`
- [ ] (2) Update `<TargetFramework>net6.0</TargetFramework>` ? `<TargetFramework>net10.0</TargetFramework>` in `Infrastructure\Infrastructure.csproj`
- [ ] (3) Update `<TargetFramework>net6.0</TargetFramework>` ? `<TargetFramework>net10.0</TargetFramework>` in `Services\Services.csproj`
- [ ] (4) Update `<TargetFramework>net6.0</TargetFramework>` ? `<TargetFramework>net10.0</TargetFramework>` in `HermesEyes.com\HermesEyes.com.csproj`

**Verification**: All 4 .csproj files contain `<TargetFramework>net10.0</TargetFramework>`

---

### [ ] TASK-003: Update NuGet Packages
**Scope**: All 4 projects
**References**: Plan §5

**Actions:**
- [ ] (1) Update `Microsoft.EntityFrameworkCore.Abstractions` 6.0.2 ? 10.0.3 in `Domaine\Domaine.csproj`
- [ ] (2) Update `Newtonsoft.Json` 13.0.1 ? 13.0.4 in `Domaine\Domaine.csproj`
- [ ] (3) Update `Microsoft.EntityFrameworkCore` 6.0.2 ? 10.0.3 in `Infrastructure\Infrastructure.csproj`
- [ ] (4) Update `Microsoft.EntityFrameworkCore.Design` 6.0.2 ? 10.0.3 in `Infrastructure\Infrastructure.csproj`
- [ ] (5) Update `Microsoft.EntityFrameworkCore.SqlServer` 6.0.2 ? 10.0.3 in `Infrastructure\Infrastructure.csproj`
- [ ] (6) Update `Microsoft.EntityFrameworkCore.Tools` 6.0.2 ? 10.0.3 in `Infrastructure\Infrastructure.csproj`
- [ ] (7) Update `Newtonsoft.Json` 13.0.1 ? 13.0.4 in `Services\Services.csproj`
- [ ] (8) Update `Microsoft.EntityFrameworkCore.Design` 6.0.2 ? 10.0.3 in `HermesEyes.com\HermesEyes.com.csproj`

**Verification**: `dotnet restore` completes with no errors or version conflicts

---

### [ ] TASK-004: Fix Breaking Change BC-002 — Replace SHA1Managed
**Scope**: Infrastructure project
**References**: Plan §6 BC-002

**Actions:**
- [ ] (1) In `Infrastructure\APIs\Extensions\StringExtensions.cs` (line 11): Replace `new SHA1Managed().ComputeHash(data)` with `SHA1.Create().ComputeHash(data)`
- [ ] (2) Update `using` directives if needed (ensure `using System.Security.Cryptography;` is present, remove `SHA1Managed`-specific usings if any)

**Verification**: File compiles without errors or obsolete warnings related to SHA1Managed

---

### [ ] TASK-005: Fix Breaking Change BC-003 — Replace WebClient with HttpClient
**Scope**: Services project
**References**: Plan §6 BC-003

**Actions:**
- [ ] (1) In `Services\DataServices\VinRushScrapper.cs` (line 32): Replace `WebClient` usage with `HttpClient`
- [ ] (2) Update method signatures to `async` if required by `HttpClient` async APIs
- [ ] (3) Update `using` directives: add `System.Net.Http`, remove `System.Net` if no longer needed

**Verification**: File compiles without errors or obsolete warnings related to WebClient

---

### [ ] TASK-006: Fix Breaking Change BC-001 — Fix ConfigurationBinder.Get<T>()
**Scope**: HermesEyes.com project
**References**: Plan §6 BC-001

**Actions:**
- [ ] (1) In `HermesEyes.com\Model\TokensProvider.cs` (line 36): Fix `ConfigurationBinder.Get<T>()` call — ensure `Microsoft.Extensions.Configuration.Binder` package is referenced or use `Bind()` pattern if needed
- [ ] (2) Verify the fix compiles correctly

**Verification**: File compiles without errors related to ConfigurationBinder

---

### [ ] TASK-007: Build, Validate & Commit
**Scope**: Solution-wide
**References**: Plan §8, §10, §11

**Actions:**
- [ ] (1) Build the entire solution — target: 0 errors
- [ ] (2) Fix any remaining compilation errors found during build (following dependency order: Domaine ? Infrastructure ? Services ? HermesEyes.com)
- [ ] (3) Verify all success criteria from Plan §11 are met
- [ ] (4) Commit all changes: `chore: upgrade solution from .NET 6.0 to .NET 10.0`

**Verification**: Solution builds with 0 errors; all changes committed on `upgrade-to-NET10` branch
