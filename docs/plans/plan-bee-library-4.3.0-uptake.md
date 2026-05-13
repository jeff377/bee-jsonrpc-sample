# 計畫：因應 bee-library / bee-ui-core 4.3.0 升級 bee-jsonrpc-sample

**狀態**：待使用者確認

## 背景

bee-library 與 bee-ui-core 同步發佈 v4.3.0，主要變更：

1. **新增 `Bee.Hosting` 套件** — DI composition root，不依賴 ASP.NET Core
2. **`AddBeeFramework` 從 `Bee.Api.AspNetCore` 搬至 `Bee.Hosting`**
3. **4.3.0 server 端 startup 改採「PathOptions + SystemSettingsLoader + SysInfo.Initialize + AddBeeFramework + UseBeeFramework」標準流程**（取代 4.1.0 時代散落的 `BackendInfo.DefineAccess = new LocalDefineAccess()` / `BackendInfo.Initialize` / `ApiContractRegistry.Register<...>` 手動寫法）
4. **`ApiContractRegistry.Register` 不再需要手動呼叫**（ADR-007 採命名慣例自動轉換，`ApiContractRegistry` 只剩 MessagePack Typeless 序列化白名單職責，框架自動處理）
5. **`DbProviderRegistry.Register` 仍由宿主負責**（框架不自動註冊 ADO.NET provider）
6. **bee-ui-core 4.3.0 移除 `BackendInfo` 在 client surface 的暴露**；非 web 宿主走近端模式時必須先 `services.AddBeeFramework(...)` 並設 `ApiClientInfo.LocalServiceProvider = sp`

詳細：
- [bee-library v4.3.0 CHANGELOG](https://github.com/jeff377/bee-library/blob/v4.3.0/CHANGELOG.md)
- [bee-library v4.3.0 plan-extract-bee-hosting.md](https://github.com/jeff377/bee-library/blob/v4.3.0/docs/plans/plan-extract-bee-hosting.md)
- [bee-library Development Cookbook（startup flow）](https://github.com/jeff377/bee-library/blob/main/docs/development-cookbook.md#framework-initialization-order)
- [bee-ui-core 4.3.0 uptake plan](https://github.com/jeff377/bee-ui-core/blob/main/docs/plans/plan-bee-library-4.3.0-uptake.md)

## Explore 結論

### 角色對應

| 角色 | 專案 | 現況 | 4.3.0 後 |
|---|---|---|---|
| **Server**（ASP.NET Core 宿主） | `src/JsonRpcServer/` | 自製 `app.BackendInitialize(...)`，手動呼叫 `BackendInfo.DefineAccess = new LocalDefineAccess()` / `BackendInfo.Initialize` / `ApiServiceOptions.Initialize` / `DbProviderRegistry.Register` / `ApiContractRegistry.Register` | 改用 `AddBeeFramework` + `UseBeeFramework` 標準流程；保留 `DbProviderRegistry.Register`（SQL Server）；移除 `ApiContractRegistry.Register`（不再需要）；自製 `BackendInitialize` extension method 刪除 |
| **Client**（跨平台 console，純遠端 demo） | `src/JsonRpcClient/` | ref `Bee.UI.Core` 4.1.0，用 `ClientInfo.Initialize(endpoint)`（遠端 URL）→ Login → 呼叫 BO API。Menu 6 「HelloLocal」實際是遠端 HTTP 打 server 上 `[LocalOnly]` BO，demo 拒絕回應 | 只升 `Bee.UI.Core` 4.1.0 → 4.3.0；**不**新增 `Bee.Hosting` ref；程式碼若無 API surface 異動則無變動 |
| **Custom.Api.Contracts** | `src/Custom.Api.Contracts/` | 純介面，無套件相依 | 不變 |
| **Custom.Api** | `src/Custom.Api/` | `Bee.Api.Core` 4.1.0 | 升 4.3.0 |
| **Custom.Business** | `src/Custom.Business/` | `Bee.Business` 4.1.0 + `Bee.Repository` 4.1.0 | 升 4.3.0 |
| ~~Custom.Contracts~~ | `src/Custom.Contracts/` | 已不在 slnx，只剩 bin/obj 殘留 | **刪除整個資料夾** |

### 套件相依現況（PackageReference）

| 專案 | 套件 | 版本 |
|---|---|---|
| `JsonRpcServer` | `Bee.Api.AspNetCore` | 4.1.0 → 4.3.0 |
| `JsonRpcServer` | `Bee.ObjectCaching` | 4.1.0 → **直接刪除**（已遞移自 `Bee.Api.AspNetCore` → `Bee.Hosting`，不必顯式 ref） |
| `JsonRpcServer` | `Microsoft.Data.SqlClient` | 6.1.3（不動） |
| `JsonRpcClient` | `Bee.UI.Core` | 4.1.0 → 4.3.0 |
| `Custom.Api` | `Bee.Api.Core` | 4.1.0 → 4.3.0 |
| `Custom.Business` | `Bee.Business` | 4.1.0 → 4.3.0 |
| `Custom.Business` | `Bee.Repository` | 4.1.0 → 4.3.0 |

> 註：`Bee.ObjectCaching` 是否要保留 explicit ref 取決於 sample 是否直接 `using Bee.ObjectCaching;`。`BackendExtensions.cs` 有 `using Bee.ObjectCaching;`，但實際上沒用到任何符號（只是舊 sample 殘留）。新版 `BackendExtensions.cs` 整個會刪除，新 `Program.cs` 也不會用到，所以 `Bee.ObjectCaching` PackageReference 可以一併移除（透過 `Bee.Api.AspNetCore → Bee.Hosting → Bee.ObjectCaching` 遞移帶入仍可供應）。

## 影響檔案清單

### 異動（檔案層級）

1. **`src/JsonRpcServer/JsonRpcServer.csproj`**
   - `Bee.Api.AspNetCore` 4.1.0 → 4.3.0
   - 移除 `Bee.ObjectCaching` 4.1.0 PackageReference（遞移帶入即可）

2. **`src/JsonRpcServer/Program.cs`**
   - 改寫成 4.3.0 標準 startup flow：
     - 從 `IConfiguration["DefinePath"]` 讀 DefinePath，做相對→絕對路徑轉換、確認目錄存在
     - 建立 `PathOptions { DefinePath = absolutePath }`
     - `var settings = SystemSettingsLoader.Load(paths)`
     - `SysInfo.Initialize(settings.CommonConfiguration)`
     - `services.AddBeeFramework(settings.BackendConfiguration, paths, autoCreateMasterKey: true)`
     - `DbProviderRegistry.Register(DatabaseType.SQLServer, SqlClientFactory.Instance)` — 仍須保留
     - `builder.Services.AddControllers()`
     - `app.UseBeeFramework()`
     - `app.MapControllers()`
   - `using` 加入：`Bee.Hosting`、`Bee.Api.AspNetCore`、`Bee.Base`、`Bee.Db.Manager`、`Bee.Definition`、`Bee.Definition.Database`、`Bee.Definition.Settings`、`Microsoft.Data.SqlClient`

3. **`src/JsonRpcServer/Extensions/BackendExtensions.cs`** → **整個刪除**
   - 原本的 `BackendInitialize` 邏輯全部搬進 `Program.cs`（startup flow 已扁平化，沒有 facade 必要）
   - `BackendInfo.DefineAccess = new LocalDefineAccess()` 不再需要（由 `AddBeeFramework` 內部依 `SystemSettings.xml` 的 `BackendComponents.DefineAccess` 設定建立並註冊到 DI）
   - `BackendInfo.Initialize(...)` 不再需要（4.3.0 後框架不再透過 `BackendInfo` 靜態狀態運作）
   - `ApiServiceOptions.Initialize(...)` 不再需要（`SysInfo.Initialize` 內部處理 process-wide payload options）
   - `ApiContractRegistry.Register<IHelloResponse, HelloResponse>()` 不再需要（ADR-007 命名慣例自動轉換）

4. **`src/JsonRpcServer/Controllers/ApiServiceController.cs`** — 不動
   - 仍是 `class ApiServiceController : Bee.Api.AspNetCore.Controllers.ApiServiceController`（基類在 4.3.0 仍存在，且改用 DI ctor 解析 `JsonRpcExecutor`，繼承端不受影響）

5. **`src/JsonRpcClient/JsonRpcClient.csproj`**
   - `Bee.UI.Core` 4.1.0 → 4.3.0

6. **`src/JsonRpcClient/Program.cs`** — 不動
   - `ClientInfo.Initialize(endpoint)` / `ClientInfo.SystemApiConnector` / `ClientInfo.CreateFormApiConnector` API 在 4.3.0 簽章不變
   - 不引入 `Bee.Hosting` ref（純遠端 demo）

7. **`src/Custom.Api/Custom.Api.csproj`**
   - `Bee.Api.Core` 4.1.0 → 4.3.0

8. **`src/Custom.Business/Custom.Business.csproj`**
   - `Bee.Business` 4.1.0 → 4.3.0
   - `Bee.Repository` 4.1.0 → 4.3.0

9. **`src/Custom.Api.Contracts/Custom.Api.Contracts.csproj`** — 不動（無套件相依）

10. **`src/Custom.Business/*.cs`** — 預期不動（`BusinessArgs` / `BusinessResult` / `FormBusinessObject` 公開 API 在 4.3.0 預期未變；若 build 失敗再個案處理）
    - `BusinessObjectFactory` 在 4.3.0 框架走 DI 後可能改為 ctor inject（如 `IServiceProvider` / `IDefineAccess` / `ISessionInfoService` / `IFormBoTypeResolver`）；目前的 parameterless ctor 是否仍能被 `AddBeeFramework` 接受需在 build 階段驗證。**若 build 失敗，預期改動範圍**：
      - `BusinessObjectFactory` 改為接受框架要求的 ctor 參數（通常 `IServiceProvider`、`IDefineAccess`、`ISessionInfoService`、`IFormBoTypeResolver`）
      - 透過 `SystemSettings.xml` 的 `BackendComponents.BusinessObjectFactory` 指定自訂型別（已有設定，現況 `Define/SystemSettings.xml` 已指向 `Custom.Business.BusinessObjectFactory`）
    - 視 build 結果決定，先列為「潛在風險點」不在 plan 確定範圍

11. **`src/Custom.Contracts/`** — **整個資料夾刪除**（slnx 已不含，bin/obj 殘留）

12. **`README.md`**
    - 「NuGet Dependencies」段落版本 `v4.1.0` → `v4.3.0`
    - 在 dependency 清單加上 `Bee.Hosting` 註解（說明它是透過 `Bee.Api.AspNetCore` 遞移帶入，並指出 startup 必須 `using Bee.Hosting;`）
    - 「Getting Started」可加一小段「4.3.0 升版備註」描述新 startup pattern（或寫一條 link 指向 development-cookbook）

13. **`CHANGELOG.md`** — **新建**
    - 起步 entry：`## [Unreleased] — Bee.NET 4.3.0 uptake`
    - 列出本次升版重點：套件版本對齊、Program.cs 改用 `AddBeeFramework` + `UseBeeFramework`、移除自製 `BackendExtensions`、移除 `ApiContractRegistry.Register` 手動呼叫

### 不動

- `src/Custom.Api/HelloRequest.cs` / `HelloResponse.cs`（`ApiRequest` / `ApiResponse` 基類 4.3.0 未變）
- `src/Custom.Business/HelloArgs.cs` / `HelloResult.cs` / `EmployeeBusinessObject.cs`（前提：`BusinessArgs` / `BusinessResult` / `FormBusinessObject` 簽章未變）
- `src/JsonRpcServer/Controllers/ApiServiceController.cs`
- `src/JsonRpcServer/appsettings.json` / `appsettings.Development.json` / `Properties/launchSettings.json`
- `src/Define/*`（SystemSettings.xml 等設定不動）
- `bee-jsonrpc-sample.slnx`

## 嚴禁事項（符合任務描述）

- **不**在 `JsonRpcClient` 加 `Bee.Hosting` 的 PackageReference
- **不**修改 `LocalApiProvider` 反射機制（沒理由動，本 repo 也沒直接接觸）
- **不**為了「方便 demo」把 `Custom.Business` 直接 ref 進 `JsonRpcClient`

## 驗收標準

- [ ] `dotnet restore` 在 macOS 本機成功
- [ ] `dotnet build --configuration Release` 全綠（5 個專案）
- [ ] `dotnet build` 在 `TreatWarningsAsErrors=true` 下 0 warning 0 error（若 csproj 沒明確設，採 `Directory.Build.props` 或預設值）
- [ ] `dotnet test` —— 本 repo 無 tests，N/A
- [ ] `JsonRpcServer` 本機啟動成功（`dotnet run --project src/JsonRpcServer`），listening port 不變（沿用 `launchSettings.json`）
- [ ] `JsonRpcClient` 本機啟動成功，依序 demo：
  - [ ] menu 1（Initialize endpoint = `http://localhost:5219/api`）
  - [ ] menu 2（Login，回傳 access token）
  - [ ] menu 3（Hello plain — 應回 `Hello, Jeff`）
  - [ ] menu 4（HelloEncoded — 應回 `[Encoded & Auth] Hello, Jeff`）
  - [ ] menu 5（HelloEncrypted — 應回 `[Encrypted & Auth] Hello, Jeff`）
  - [ ] menu 6（HelloLocal — 應回 protocol-level 拒絕，因 `[LocalOnly]`）
- [ ] `JsonRpcClient` 輸出目錄 `bin/Release/net10.0/` **不**含 `Microsoft.AspNetCore.*.dll`（驗證純 client 沒被 Bee.Hosting 拖入）
- [ ] `JsonRpcServer` 輸出目錄含 `Bee.Hosting.dll`（透過 `Bee.Api.AspNetCore` 遞移帶入）
- [ ] grep `BackendInfo` / `BackendInitialize` / `ApiServiceOptions.Initialize` / `ApiContractRegistry.Register` 在 `src/` 為 0 命中
- [ ] grep `Bee.Hosting` 在 `JsonRpcServer/Program.cs` 為 1 命中（using）；在 `JsonRpcClient/` 為 0 命中

## 待確認事項

### 待確認 1：CHANGELOG.md 是否需建立？

- **選項 A（推薦）**：新建 `CHANGELOG.md`，留 `## [Unreleased]` entry 描述本次升版內容
- **選項 B**：不建，commit message + PR description 自足
- **選項 C**：等真的需要對外發版 / 標 tag 時再建

> 預設取 A（與 bee-library / bee-ui-core 慣例對齊）

### 待確認 2：`BusinessObjectFactory` 若被 build 揭露需 ctor 改動，行動策略？

- **選項 A（推薦）**：先按 plan 升版號，等 `dotnet build` 揭露要改的 ctor 後再改動。改動原則：依 4.3.0 框架要求的 ctor 簽章調整、保持實作邏輯不變（仍 switch progId 路由 `EmployeeBusinessObject` vs `FormBusinessObject`）
- **選項 B**：plan 階段就先 fetch 4.3.0 內 `IBusinessObjectFactory` 與 `BusinessObjectFactory` 預設實作的 ctor，事先在 plan 中列出 ctor 變動

> 預設取 A（plan 階段保持彈性，避免猜測；4.3.0 預設 BO factory 是 ActivatorUtilities-aware，sample 改動以實際 build 失敗訊息為準）

### 待確認 3：本次工作流選 PR 還是直接 main？

依 `~/.claude/rules/pull-request.md`：
- macOS 桌面可本機 `dotnet build` + 啟動 server/client demo 驗證 → **預設直接 commit 到 main + push**
- push 後遠端 CI（如有設）監看

> 預設取「直接 commit 到 main」；CI 失敗即時修復。

### 待確認 4：是否同步加雙語 README？

- **選項 A**：維持英文單一 `README.md`（現況）
- **選項 B**：新增 `README.zh-TW.md` 並在兩份頂部加語言切換連結（與 bee-library / bee-ui-core 慣例對齊）
- **選項 C**：延後到下個 PR

> 預設取 A（不擴大本次任務範圍）；若使用者偏好對齊 bee-library 慣例則改 B。

## 執行流程（plan 通過後）

1. 依「待確認」回覆套用設定
2. 刪 `src/Custom.Contracts/`
3. 改 6 個 csproj 的 PackageReference 版本
4. 改寫 `src/JsonRpcServer/Program.cs`、刪 `src/JsonRpcServer/Extensions/BackendExtensions.cs`（連同 `Extensions` 子資料夾若清空也刪）
5. 本機 `dotnet restore` + `dotnet build --configuration Release`
6. 若有 build 錯誤：依錯誤訊息收斂修正（最可能落在 `Custom.Business.BusinessObjectFactory` ctor 簽章）
7. 啟動 `JsonRpcServer`，跑 `JsonRpcClient` menu 1-6 一輪
8. 檢查 `JsonRpcClient` 輸出目錄不含 `Microsoft.AspNetCore.*.dll`
9. 更新 `README.md`（依 待確認 4 決定是否加雙語）
10. 新建 `CHANGELOG.md`（依 待確認 1）
11. `git add` 改動的檔案 + `docs/plans/plan-bee-library-4.3.0-uptake.md`
12. `git commit` + `git push origin main`（或建分支 + PR，依 待確認 3）
13. 遠端 CI 監看（如有設）

## 不在範圍內

- **不**重構任何 BO / API 型別邏輯（升版 only）
- **不**改 `Define/*.xml` 設定檔（除非 4.3.0 強制要求新欄位）
- **不**新增近端連線 demo（依使用者決策，client 只 demo 遠端）
- **不**動 `LocalApiProvider` 反射機制
- **不**升 `Microsoft.Data.SqlClient`（與本次 4.3.0 升版無關）
- **不**動 `.editorconfig` / `.gitignore` / `LICENSE`
- bee-library / bee-ui-core 上游本身不在本 repo 範圍
