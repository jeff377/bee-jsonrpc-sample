# Bee.NET JSON-RPC Sample

JSON-RPC 2.0 server and client sample applications built with the [Bee.NET](https://github.com/jeff377/bee-library) framework.

Demonstrates how to use the Connector for both local and remote connections, and how to lay out a custom application following Bee.NET's three-assembly architecture for separating API contracts from business-object (BO) implementations.

## Projects

| Project | Role | Description |
|---------|------|-------------|
| **JsonRpcServer** | Host | ASP.NET Core JSON-RPC 2.0 API server |
| **JsonRpcClient** | Host | Cross-platform console client application |
| **Custom.Api.Contracts** | Contracts | Pure interfaces (`IHelloRequest` / `IHelloResponse`); no framework dependency |
| **Custom.Api** | API types | `HelloRequest` / `HelloResponse` with `[MessagePackObject]`, used over the wire |
| **Custom.Business** | BO types + logic | `HelloArgs` / `HelloResult` (POCO), `EmployeeBusinessObject`, `BusinessObjectFactory` |
| **Define** | — | Configuration and schema files |

### Architecture

```
Custom.Api.Contracts  ── interfaces only
        ├── Custom.Api        ── API types (Bee.Api.Core)         ─── JsonRpcClient
        └── Custom.Business   ── BO types + logic (Bee.Business)  ─── JsonRpcServer
                                                                       (also refs Custom.Api
                                                                        to register contract
                                                                        mapping at startup)
```

`Custom.Business` does **not** reference `Custom.Api` — the contract-interface assembly is the only bridge between API and BO. See [api-bo-contract-design.md](https://github.com/jeff377/bee-library/blob/main/docs/api-bo-contract-design.md) in the framework repo for the full design rationale.

The server registers BO-to-API type conversion at startup so that BOs can return pure POCOs:

```csharp
ApiContractRegistry.Register<IHelloResponse, HelloResponse>();
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (for JsonRpcServer)

## Getting Started

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build --configuration Release
```

## NuGet Dependencies

This sample references the following Bee.NET NuGet packages (v4.1.0):

- Bee.Api.AspNetCore (server)
- Bee.Api.Core (Custom.Api)
- Bee.Business (Custom.Business)
- Bee.ObjectCaching (server)
- Bee.Repository (Custom.Business)
- Bee.UI.Core (client)

## License

MIT License - see [LICENSE](LICENSE) for details.
