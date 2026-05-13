# Changelog

All notable changes to this sample are documented here.

## [Unreleased] — Bee.NET 4.3.0 uptake

### Changed

- Upgraded all `Bee.*` NuGet packages from **4.1.0 → 4.3.0**:
  - `Bee.Api.AspNetCore` (JsonRpcServer) — now transitively brings in `Bee.Hosting`, `Bee.ObjectCaching`, `Bee.Business`, `Bee.Repository`
  - `Bee.Api.Core` (Custom.Api)
  - `Bee.Business` + `Bee.Repository` (Custom.Business)
  - `Bee.UI.Core` (JsonRpcClient)
- **Server startup migrated to the 4.3.0 composition-root pattern** (`JsonRpcServer/Program.cs`):
  - `PathOptions` + `SystemSettingsLoader.Load` + `SysInfo.Initialize` + `services.AddBeeFramework(...)` + `app.UseBeeFramework()`
  - Replaces the 4.1.0-style manual chain (`BackendInfo.DefineAccess = new LocalDefineAccess()`, `BackendInfo.Initialize`, `ApiServiceOptions.Initialize`, `ApiContractRegistry.Register`)
- `Custom.Business.BusinessObjectFactory` rewritten to constructor-inject `IServiceProvider` / `IDefineAccess` / `ISessionInfoService` and build the per-call `BeeContext`, aligning with the 4.3.0 BO factory contract (`ActivatorUtilities.CreateInstance`-friendly).
- `EmployeeBusinessObject` ctor signature updated to `(IBeeContext ctx, Guid accessToken, string progId, bool isLocalCall)` to match the new `FormBusinessObject` base class.
- `Bee.ObjectCaching` PackageReference removed from `JsonRpcServer.csproj` — it is now pulled in transitively via `Bee.Api.AspNetCore → Bee.Hosting`.

### Removed

- `JsonRpcServer/Extensions/BackendExtensions.cs` — manual `BackendInitialize` extension is no longer needed; `AddBeeFramework` covers definition access, cache wiring, repository factories, business-object factory, and `JsonRpcExecutor`.
- `src/Custom.Contracts/` — empty orphan folder left behind by the contract-separation refactor (`5dff40c`). It was no longer in the solution.
- Manual `ApiContractRegistry.Register<IHelloResponse, HelloResponse>()` — 4.3.0 derives the BO → API type mapping by naming convention (ADR-007); `ApiContractRegistry` is now only the MessagePack Typeless serialization whitelist.

### Fixed

- `Define/SystemSettings.xml`: `<AllowedTypeNamespaces>` corrected from the obsolete `Custom.Contracts` to the current `Custom.Api.Contracts` (leftover from the contract-separation refactor).

### Notes

- `JsonRpcClient` deliberately keeps its package surface to `Bee.UI.Core` only. It does not reference `Bee.Hosting` — the client is a pure remote-mode demo. Verified: `bin/Release/net10.0/` contains no `Microsoft.AspNetCore.*.dll`.
- Server `bin/Release/net10.0/` includes `Bee.Hosting.dll` (transitively, as expected).
