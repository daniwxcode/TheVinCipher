# Plan de migration .NET 6.0 ? .NET 10.0 — Solution HermesEyes

## Table of Contents

- [1. Executive Summary](#1-executive-summary)
- [2. Migration Strategy](#2-migration-strategy)
- [3. Detailed Dependency Analysis](#3-detailed-dependency-analysis)
- [4. Project-by-Project Migration Plans](#4-project-by-project-migration-plans)
  - [4.1 Domaine.csproj](#41-domainecsproj)
  - [4.2 Infrastructure.csproj](#42-infrastructurecsproj)
  - [4.3 Services.csproj](#43-servicescsproj)
  - [4.4 HermesEyes.com.csproj](#44-hermeseyescomcsproj)
- [5. Package Update Reference](#5-package-update-reference)
- [6. Breaking Changes Catalog](#6-breaking-changes-catalog)
- [7. Risk Management](#7-risk-management)
- [8. Testing & Validation Strategy](#8-testing--validation-strategy)
- [9. Complexity & Effort Assessment](#9-complexity--effort-assessment)
- [10. Source Control Strategy](#10-source-control-strategy)
- [11. Success Criteria](#11-success-criteria)

---

## 1. Executive Summary

### Scenario

Upgrade de la solution **HermesEyes** de **.NET 6.0** vers **.NET 10.0 (Long Term Support)**. Les 4 projets de la solution sont actuellement sur `net6.0` et doivent être migrés vers `net10.0` simultanément.

### Scope

| Metric | Value |
|:---|:---|
| Total Projects | 4 (all require upgrade) |
| Total NuGet Packages | 12 (6 need upgrade) |
| Total Code Files | 63 |
| Total Lines of Code | 5,504 |
| Code Files with Incidents | 7 |
| API Issues | 4 (2 binary incompatible, 2 source incompatible) |
| Security Vulnerabilities | 0 |
| Test Projects | 0 |

### Selected Strategy

**All-at-Once Strategy** — All 4 projects upgraded simultaneously in a single atomic operation.

**Rationale**:
- 4 projects (small solution, well under the ?5 threshold)
- All currently on .NET 6.0 (homogeneous codebase)
- Clear, linear dependency structure with no circular dependencies
- All ?? Low difficulty — no high-risk projects
- All 6 packages requiring upgrade have known target versions available
- No security vulnerabilities to complicate the upgrade
- Total codebase is only 5,504 LOC — minimal risk surface

### Critical Issues

- **Binary Incompatible API** : `ConfigurationBinder.Get<T>()` — 2 occurrences dans `HermesEyes.com\Model\TokensProvider.cs` (ligne 36). Nécessite modification de code obligatoire.
- **Source Incompatible APIs** :
  - `System.Security.Cryptography.SHA1Managed` — 1 occurrence dans `Infrastructure\APIs\Extensions\StringExtensions.cs` (ligne 11). Algorithme cryptographique obsolète.
  - `System.Net.WebClient` — 1 occurrence dans `Services\DataServices\VinRushScrapper.cs` (ligne 32). API réseau obsolète.

### Complexity Classification

**Simple** — 4 projets, profondeur de dépendance ? 3, aucun haut risque, aucune vulnérabilité. Plan généré avec un nombre réduit d'itérations (batch rapide).

---

## 2. Migration Strategy

### Approach: All-at-Once — Unified Atomic Upgrade

La mise à niveau sera réalisée comme une **opération atomique unique**. Tous les fichiers projets, toutes les références de packages, et toutes les corrections de code sont effectuées en une seule passe coordonnée, sans états intermédiaires.

### Justification

| Criteria | Assessment | Decision |
|:---|:---|:---|
| Solution size | 4 projects (< 5) | ? Favorable for All-at-Once |
| Dependency complexity | Linear chain, no cycles | ? Simple resolution |
| Codebase size | 5,504 LOC total | ? Small, manageable |
| Risk level | All projects ?? Low | ? Low overall risk |
| Package updates | 6 packages, all with known versions | ? Straightforward |
| Test coverage | No test projects in solution | ?? Manual testing needed |
| Breaking changes | 4 issues, all well-documented | ? Predictable |

### Implementation Timeline

#### Phase 0: Preparation
- Verify .NET 10 SDK installation
- Validate `global.json` compatibility (if present)

#### Phase 1: Atomic Upgrade
**Operations** (performed as single coordinated batch):
- Update all 4 project files to `net10.0`
- Update all 6 package references across all projects
- Restore dependencies
- Build solution and fix all compilation errors (including breaking API changes)

**Deliverables**: Solution builds with 0 errors

#### Phase 2: Validation
**Operations**:
- Verify clean build (0 errors, 0 warnings where possible)
- No test projects exist — recommend manual smoke testing post-upgrade

**Deliverables**: Solution compiles successfully, no dependency conflicts

### Dependency-Based Ordering

Although this is an atomic operation, understanding the dependency order is essential for troubleshooting compilation errors:

1. **Domaine.csproj** — Leaf node (0 dependencies) — resolve first
2. **Infrastructure.csproj** — Depends on Domaine
3. **Services.csproj** — Depends on Domaine + Infrastructure
4. **HermesEyes.com.csproj** — Root node, depends on Services (transitive to all)

---

## 3. Detailed Dependency Analysis

### Dependency Graph Summary

```
Domaine.csproj (leaf)
??? ? Infrastructure.csproj
?       ??? ? Services.csproj
?       ?       ??? ? HermesEyes.com.csproj (root)
?       ???????????????????????????????????? (via Services)
??? ? Services.csproj (direct)
```

### Project Grouping

Since the All-at-Once strategy is selected, all 4 projects form a **single upgrade group**:

| Project | Type | Dependencies | Dependants | Role |
|:---|:---|:---:|:---:|:---|
| Domaine.csproj | ClassLibrary | 0 | 2 | Leaf node — domain model |
| Infrastructure.csproj | ClassLibrary | 1 (Domaine) | 1 | Data access layer |
| Services.csproj | ClassLibrary | 2 (Domaine, Infrastructure) | 1 | Business logic |
| HermesEyes.com.csproj | AspNetCore | 1 (Services) | 0 | Root — Web application |

### Critical Path

`Domaine ? Infrastructure ? Services ? HermesEyes.com`

This is the longest dependency chain and determines compilation order when troubleshooting build errors.

### Circular Dependencies

**None detected.** The dependency graph is a clean DAG (Directed Acyclic Graph).

---

## 4. Project-by-Project Migration Plans

### 4.1 Domaine.csproj

**Current State**

| Property | Value |
|:---|:---|
| Target Framework | net6.0 |
| Project Kind | ClassLibrary (SDK-style) |
| Dependencies | 0 project dependencies |
| Dependants | 2 (Infrastructure, Services) |
| Files | 9 |
| Lines of Code | 444 |
| Risk Level | ?? Low |
| API Issues | 0 |
| Package Issues | 2 |

**Target State**: `net10.0`

**Migration Steps**:

1. **Update TargetFramework** in `Domaine\Domaine.csproj`:
   - Change `<TargetFramework>net6.0</TargetFramework>` ? `<TargetFramework>net10.0</TargetFramework>`

2. **Update Package References**:
   | Package | Current | Target | Reason |
   |:---|:---:|:---:|:---|
   | Microsoft.EntityFrameworkCore.Abstractions | 6.0.2 | 10.0.3 | Framework alignment |
   | Newtonsoft.Json | 13.0.1 | 13.0.4 | Recommended update |

3. **Expected Breaking Changes**: None — 0 API issues detected, all 954 APIs analyzed are compatible.

4. **Validation Checklist**:
   - [ ] Builds without errors
   - [ ] No package dependency conflicts

---

### 4.2 Infrastructure.csproj

**Current State**

| Property | Value |
|:---|:---|
| Target Framework | net6.0 |
| Project Kind | ClassLibrary (SDK-style) |
| Dependencies | 1 (Domaine) |
| Dependants | 1 (Services) |
| Files | 30 |
| Lines of Code | 2,615 |
| Risk Level | ?? Low |
| API Issues | 1 (Source Incompatible) |
| Package Issues | 4 |

**Target State**: `net10.0`

**Migration Steps**:

1. **Update TargetFramework** in `Infrastructure\Infrastructure.csproj`:
   - Change `<TargetFramework>net6.0</TargetFramework>` ? `<TargetFramework>net10.0</TargetFramework>`

2. **Update Package References**:
   | Package | Current | Target | Reason |
   |:---|:---:|:---:|:---|
   | Microsoft.EntityFrameworkCore | 6.0.2 | 10.0.3 | Framework alignment |
   | Microsoft.EntityFrameworkCore.Design | 6.0.2 | 10.0.3 | Framework alignment |
   | Microsoft.EntityFrameworkCore.SqlServer | 6.0.2 | 10.0.3 | Framework alignment |
   | Microsoft.EntityFrameworkCore.Tools | 6.0.2 | 10.0.3 | Framework alignment |

3. **Expected Breaking Changes**:

   **`System.Security.Cryptography.SHA1Managed`** — Source Incompatible
   - **File**: `Infrastructure\APIs\Extensions\StringExtensions.cs`, line 11
   - **Current code**: `byte[]? hashData = new SHA1Managed().ComputeHash(data);`
   - **Issue**: `SHA1Managed` is obsoleted in .NET 10.0. The type produces compiler warnings/errors as it has been deprecated for security reasons.
   - **Fix**: Replace `new SHA1Managed()` with `SHA1.Create()`. Use the factory method `System.Security.Cryptography.SHA1.Create()` which returns a platform-optimized implementation.
   - **Updated code**: `byte[]? hashData = SHA1.Create().ComputeHash(data);`
   - ?? **Security note**: SHA1 itself is considered weak. If the use case allows, consider migrating to `SHA256.Create()` for stronger security.

4. **EF Core 6 ? 10 Considerations**:
   - EF Core 10 may introduce breaking changes in migration format, query pipeline, and provider APIs.
   - Verify that existing EF migrations are compatible. May need to regenerate or update migration snapshots.
   - Review `DbContext` configuration for deprecated options.

5. **Validation Checklist**:
   - [ ] Builds without errors
   - [ ] SHA1Managed replacement compiles
   - [ ] No EF Core runtime exceptions (verify at application level)
   - [ ] No package dependency conflicts

---

### 4.3 Services.csproj

**Current State**

| Property | Value |
|:---|:---|
| Target Framework | net6.0 |
| Project Kind | ClassLibrary (SDK-style) |
| Dependencies | 2 (Domaine, Infrastructure) |
| Dependants | 1 (HermesEyes.com) |
| Files | 12 |
| Lines of Code | 1,697 |
| Risk Level | ?? Low |
| API Issues | 1 (Source Incompatible) |
| Package Issues | 1 |

**Target State**: `net10.0`

**Migration Steps**:

1. **Update TargetFramework** in `Services\Services.csproj`:
   - Change `<TargetFramework>net6.0</TargetFramework>` ? `<TargetFramework>net10.0</TargetFramework>`

2. **Update Package References**:
   | Package | Current | Target | Reason |
   |:---|:---:|:---:|:---|
   | Newtonsoft.Json | 13.0.1 | 13.0.4 | Recommended update |

3. **Expected Breaking Changes**:

   **`System.Net.WebClient`** — Source Incompatible
   - **File**: `Services\DataServices\VinRushScrapper.cs`, line 32
   - **Current code**: `WebClient webClient = new WebClient();`
   - **Issue**: `WebClient` is obsoleted in .NET 10.0. The class generates compiler warnings/errors. It has been deprecated in favor of `HttpClient`.
   - **Fix**: Replace `WebClient` usage with `HttpClient`. The migration involves:
     1. Replace `new WebClient()` with an injected or locally instantiated `HttpClient`
     2. Replace synchronous download calls (e.g., `DownloadString`, `DownloadData`) with async equivalents (`GetStringAsync`, `GetByteArrayAsync`)
     3. Ensure proper `HttpClient` lifetime management (ideally use `IHttpClientFactory` or a shared static instance)
   - **Example migration**:
     ```csharp
     // Before:
     WebClient webClient = new WebClient();
     string result = webClient.DownloadString(url);

     // After:
     using HttpClient httpClient = new HttpClient();
     string result = await httpClient.GetStringAsync(url);
     ```
   - ?? **Note**: This change may require making the containing method `async`. Review callers up the call chain.

4. **Validation Checklist**:
   - [ ] Builds without errors
   - [ ] WebClient replacement compiles and functions correctly
   - [ ] No package dependency conflicts

---

### 4.4 HermesEyes.com.csproj

**Current State**

| Property | Value |
|:---|:---|
| Target Framework | net6.0 |
| Project Kind | AspNetCore (SDK-style) |
| Dependencies | 1 (Services) — transitive: Domaine, Infrastructure |
| Dependants | 0 (Root application) |
| Files | 14 |
| Lines of Code | 748 |
| Risk Level | ?? Low |
| API Issues | 2 (Binary Incompatible) |
| Package Issues | 1 |

**Target State**: `net10.0`

**Migration Steps**:

1. **Update TargetFramework** in `HermesEyes.com\HermesEyes.com.csproj`:
   - Change `<TargetFramework>net6.0</TargetFramework>` ? `<TargetFramework>net10.0</TargetFramework>`

2. **Update Package References**:
   | Package | Current | Target | Reason |
   |:---|:---:|:---:|:---|
   | Microsoft.EntityFrameworkCore.Design | 6.0.2 | 10.0.3 | Framework alignment |

3. **Expected Breaking Changes**:

   **`ConfigurationBinder.Get<T>()`** — Binary Incompatible (2 occurrences)
   - **File**: `HermesEyes.com\Model\TokensProvider.cs`, line 36
   - **Current code**: `Tokens = _configurationManager.GetSection("Tokens").Get<List<TokenInfo>>() ?? new List<TokenInfo>();`
   - **Issue**: In .NET 10.0, `ConfigurationBinder.Get<T>(IConfiguration)` requires the `Microsoft.Extensions.Configuration.Binder` package to be explicitly referenced, and the method signature may have changed. This is a **binary breaking change** — code will not compile without modification.
   - **Fix**: Ensure `Microsoft.Extensions.Configuration.Binder` NuGet package is referenced. If the API has been removed, use the alternative overload:
     ```csharp
     // Option 1: Add explicit Binder package and keep existing call
     Tokens = _configurationManager.GetSection("Tokens").Get<List<TokenInfo>>() ?? new List<TokenInfo>();

     // Option 2: Use Bind() method if Get<T>() is unavailable
     var tokens = new List<TokenInfo>();
     _configurationManager.GetSection("Tokens").Bind(tokens);
     Tokens = tokens;
     ```
   - ?? **Note**: The exact fix depends on compilation errors encountered during build. Both occurrences are on the same line (same expression).

4. **ASP.NET Core Considerations**:
   - `Program.cs` / `Startup.cs` patterns: Verify minimal hosting model compatibility
   - Middleware registration: Review for deprecated middleware APIs
   - Swashbuckle.AspNetCore 6.2.3 is marked compatible — no update needed
   - Flurl.Http 3.2.4 is marked compatible — no update needed

5. **Validation Checklist**:
   - [ ] Builds without errors
   - [ ] ConfigurationBinder.Get<T>() fix compiles
   - [ ] Application starts without runtime errors
   - [ ] No package dependency conflicts

---

## 5. Package Update Reference

### Common Package Updates (affecting multiple projects)

| Package | Current | Target | Projects Affected | Update Reason |
|:---|:---:|:---:|:---:|:---|
| Newtonsoft.Json | 13.0.1 | 13.0.4 | 2 (Domaine, Services) | Recommended update |
| Microsoft.EntityFrameworkCore.Design | 6.0.2 | 10.0.3 | 2 (HermesEyes.com, Infrastructure) | Framework alignment |

### Entity Framework Core Packages (Infrastructure project)

| Package | Current | Target | Reason |
|:---|:---:|:---:|:---|
| Microsoft.EntityFrameworkCore | 6.0.2 | 10.0.3 | Framework alignment (major version jump) |
| Microsoft.EntityFrameworkCore.SqlServer | 6.0.2 | 10.0.3 | Framework alignment |
| Microsoft.EntityFrameworkCore.Tools | 6.0.2 | 10.0.3 | Framework alignment |

### Domain Model Package (Domaine project)

| Package | Current | Target | Reason |
|:---|:---:|:---:|:---|
| Microsoft.EntityFrameworkCore.Abstractions | 6.0.2 | 10.0.3 | Framework alignment |

### Compatible Packages (no update needed)

| Package | Version | Project | Status |
|:---|:---:|:---|:---|
| Flurl.Http | 3.2.4 | HermesEyes.com | ? Compatible |
| HtmlAgilityPack | 1.11.42 | Services | ? Compatible |
| Humanizer.Core | 2.14.1 | Services | ? Compatible |
| Jint | 2.11.58 | Infrastructure | ? Compatible |
| Swashbuckle.AspNetCore | 6.2.3 | HermesEyes.com | ? Compatible |
| Vin | 0.0.5-beta | HermesEyes.com | ? Compatible |

---

## 6. Breaking Changes Catalog

### Binary Incompatible (High Impact — Require Code Changes)

#### BC-001: `ConfigurationBinder.Get<T>()` removal/change

| Property | Detail |
|:---|:---|
| **API** | `Microsoft.Extensions.Configuration.ConfigurationBinder.Get<T>(IConfiguration)` |
| **Category** | Binary Incompatible |
| **Occurrences** | 2 (same line) |
| **Project** | HermesEyes.com |
| **File** | `HermesEyes.com\Model\TokensProvider.cs` |
| **Line** | 36 |
| **Current Code** | `Tokens = _configurationManager.GetSection("Tokens").Get<List<TokenInfo>>() ?? new List<TokenInfo>();` |
| **Impact** | Code will not compile. The extension method signature has changed in the configuration binder package. |
| **Resolution** | Ensure `Microsoft.Extensions.Configuration.Binder` is referenced. If the extension method is no longer available in the same form, use `Bind()` pattern or updated overload. |

### Source Incompatible (Medium Impact — Compiler Warnings/Errors)

#### BC-002: `SHA1Managed` obsolescence

| Property | Detail |
|:---|:---|
| **API** | `System.Security.Cryptography.SHA1Managed` |
| **Category** | Source Incompatible |
| **Occurrences** | 1 |
| **Project** | Infrastructure |
| **File** | `Infrastructure\APIs\Extensions\StringExtensions.cs` |
| **Line** | 11 |
| **Current Code** | `byte[]? hashData = new SHA1Managed().ComputeHash(data);` |
| **Impact** | `SHA1Managed` is marked `[Obsolete]` with error-level severity. Will not compile with default settings. |
| **Resolution** | Replace `new SHA1Managed()` with `SHA1.Create()`. Consider upgrading to `SHA256.Create()` for improved security. |

#### BC-003: `WebClient` obsolescence

| Property | Detail |
|:---|:---|
| **API** | `System.Net.WebClient` constructor |
| **Category** | Source Incompatible |
| **Occurrences** | 1 |
| **Project** | Services |
| **File** | `Services\DataServices\VinRushScrapper.cs` |
| **Line** | 32 |
| **Current Code** | `WebClient webClient = new WebClient();` |
| **Impact** | `WebClient` is marked `[Obsolete]`. Will generate compiler warnings or errors. |
| **Resolution** | Replace `WebClient` with `HttpClient`. This may require converting synchronous methods to async. Review the full method and callers up the chain. |

---

## 7. Risk Management

### Risk Assessment Table

| Project | Risk Level | Description | Mitigation |
|:---|:---:|:---|:---|
| Domaine.csproj | ?? Low | No API issues. 2 simple package updates. 444 LOC. | Straightforward update, no special handling. |
| Infrastructure.csproj | ?? Low | 1 source-incompatible API (SHA1Managed). Major EF Core version jump (6?10). 2,615 LOC. | Replace SHA1Managed with SHA1.Create(). Verify EF Core migration compatibility post-build. |
| Services.csproj | ?? Low | 1 source-incompatible API (WebClient). 1,697 LOC. | Replace WebClient with HttpClient. May require async refactoring — review call chain. |
| HermesEyes.com.csproj | ?? Low | 2 binary-incompatible APIs (ConfigurationBinder.Get<T>). Root ASP.NET Core app. 748 LOC. | Fix configuration binding. Check ASP.NET Core hosting model compatibility. |

### Key Risk Areas

#### 1. Entity Framework Core 6 ? 10 (Infrastructure)
- **Risk**: Major version jump spanning 4 major releases. EF Core may have breaking changes in:
  - Query translation behavior
  - Migration file format
  - Provider-specific APIs (SqlServer)
  - `DbContext` configuration patterns
- **Mitigation**: Build and test database operations post-upgrade. Review EF Core release notes for versions 7, 8, 9, and 10.

#### 2. Absence de projets de test
- **Risk**: No automated tests exist in the solution. Regressions may go undetected.
- **Mitigation**: Perform manual smoke testing after successful build. Focus on:
  - Application startup
  - Database connectivity (EF Core)
  - API endpoint availability
  - Configuration loading (TokensProvider)

#### 3. WebClient ? HttpClient migration (Services)
- **Risk**: Converting from synchronous WebClient to async HttpClient may require changes propagating up the call chain.
- **Mitigation**: Review `VinRushScrapper.cs` and all callers. If async conversion is too invasive, use `HttpClient` with `.GetAwaiter().GetResult()` as a temporary bridge (not recommended for production long-term).

### Contingency Plans

| Scenario | Action |
|:---|:---|
| EF Core 10 breaks existing migrations | Regenerate migration snapshot; review migration history for compatibility |
| ConfigurationBinder fix doesn't compile | Add explicit `Microsoft.Extensions.Configuration.Binder` package; use `Bind()` pattern |
| WebClient replacement causes cascading async changes | Contain async boundary at `VinRushScrapper` level; use sync wrapper temporarily |
| Build fails with unexpected errors | Address errors following dependency order: Domaine ? Infrastructure ? Services ? HermesEyes.com |

---

## 8. Testing & Validation Strategy

### Phase Validation

Since no test projects exist in the solution, validation is focused on build success and compilation correctness.

#### Post-Atomic Upgrade Validation

1. **Build Verification**:
   - Solution builds with **0 errors**
   - Review and address any new warnings (especially obsolete API warnings)

2. **Dependency Resolution**:
   - `dotnet restore` completes without package conflicts
   - No version mismatches between projects

3. **Manual Smoke Testing** (recommended post-upgrade):
   - Application starts successfully
   - Database connectivity works (EF Core)
   - API endpoints respond correctly
   - Configuration binding loads tokens properly
   - Cryptographic operations produce expected results

### Per-Project Validation Checklist

| Project | Build | Packages | API Fixes | Notes |
|:---|:---:|:---:|:---:|:---|
| Domaine.csproj | [ ] 0 errors | [ ] 2 updated | [ ] N/A | Simplest project |
| Infrastructure.csproj | [ ] 0 errors | [ ] 4 updated | [ ] SHA1Managed fixed | Largest project (2,615 LOC) |
| Services.csproj | [ ] 0 errors | [ ] 1 updated | [ ] WebClient fixed | Review async propagation |
| HermesEyes.com.csproj | [ ] 0 errors | [ ] 1 updated | [ ] ConfigBinder fixed | Root app — final validation |

---

## 9. Complexity & Effort Assessment

### Per-Project Complexity

| Project | LOC | Package Updates | API Fixes | Dependencies | Complexity Rating |
|:---|:---:|:---:|:---:|:---:|:---:|
| Domaine.csproj | 444 | 2 | 0 | 0 | **Low** |
| Infrastructure.csproj | 2,615 | 4 | 1 (SHA1Managed) | 1 | **Low** |
| Services.csproj | 1,697 | 1 | 1 (WebClient) | 2 | **Low-Medium** |
| HermesEyes.com.csproj | 748 | 1 | 2 (ConfigBinder) | 1 | **Low** |

### Phase Complexity

| Phase | Description | Complexity |
|:---|:---|:---:|
| Phase 0: Preparation | SDK verification, global.json check | **Low** |
| Phase 1: Atomic Upgrade | TFM updates, package updates, build, fix errors | **Low-Medium** |
| Phase 2: Validation | Build verification, manual smoke testing | **Low** |

### Complexity Notes

- **Services.csproj** rated Low-Medium due to WebClient?HttpClient migration potentially requiring async changes propagating to callers.
- **Infrastructure.csproj** has the most package updates (4 EF Core packages) but SHA1Managed fix is a simple one-line replacement.
- Overall solution complexity is **Low** — this is a straightforward .NET version upgrade with well-documented breaking changes.

---

## 10. Source Control Strategy

### Branching Strategy

| Property | Value |
|:---|:---|
| Source branch | `master` |
| Upgrade branch | `upgrade-to-NET10` |
| Strategy | Single feature branch for entire upgrade |

### Commit Strategy

**Single commit approach** — All changes from the atomic upgrade (TFM updates, package updates, breaking change fixes) are committed together in one commit on the `upgrade-to-NET10` branch. This reflects the atomic nature of the All-at-Once strategy: no intermediate states, no partial upgrades.

**Recommended commit message format**:
```
chore: upgrade solution from .NET 6.0 to .NET 10.0

- Updated TargetFramework to net10.0 in all 4 projects
- Updated 6 NuGet packages (EF Core 6.0.2?10.0.3, Newtonsoft.Json 13.0.1?13.0.4)
- Fixed SHA1Managed ? SHA1.Create() in Infrastructure
- Fixed WebClient ? HttpClient in Services
- Fixed ConfigurationBinder.Get<T>() in HermesEyes.com
```

### Review and Merge Process

1. Push `upgrade-to-NET10` branch to remote
2. Create Pull Request targeting `master`
3. PR Checklist:
   - [ ] Solution builds with 0 errors
   - [ ] All 4 projects target `net10.0`
   - [ ] All 6 packages updated to target versions
   - [ ] All 3 breaking API changes resolved
   - [ ] No new security vulnerabilities introduced
   - [ ] Manual smoke testing performed
4. Code review by team member
5. Merge to `master`

---

## 11. Success Criteria

### Technical Criteria

- [ ] All 4 projects target `net10.0`
- [ ] All 6 NuGet packages updated to their target versions:
  - Microsoft.EntityFrameworkCore: 10.0.3
  - Microsoft.EntityFrameworkCore.Abstractions: 10.0.3
  - Microsoft.EntityFrameworkCore.Design: 10.0.3
  - Microsoft.EntityFrameworkCore.SqlServer: 10.0.3
  - Microsoft.EntityFrameworkCore.Tools: 10.0.3
  - Newtonsoft.Json: 13.0.4
- [ ] Solution builds with **0 errors**
- [ ] No package dependency conflicts
- [ ] No security vulnerabilities

### Breaking Changes Resolution

- [ ] `SHA1Managed` replaced with `SHA1.Create()` in `Infrastructure\APIs\Extensions\StringExtensions.cs`
- [ ] `WebClient` replaced with `HttpClient` in `Services\DataServices\VinRushScrapper.cs`
- [ ] `ConfigurationBinder.Get<T>()` fixed in `HermesEyes.com\Model\TokensProvider.cs`

### Quality Criteria

- [ ] Code quality maintained (no hack-style workarounds)
- [ ] All changes follow the All-at-Once strategy (single atomic operation)
- [ ] Commit follows recommended format on `upgrade-to-NET10` branch

### Process Criteria

- [ ] All-at-Once strategy followed — no intermediate states
- [ ] Dependency order respected during error troubleshooting
- [ ] Source control strategy followed (single branch, single commit)
