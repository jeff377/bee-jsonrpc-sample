# Bee.NET JSON-RPC Sample

JSON-RPC 2.0 server and client sample applications built with the [Bee.NET](https://github.com/jeff377/bee-library) framework.

Demonstrates how to use the Connector for both local and remote connections.

## Projects

| Project | Description |
|---------|-------------|
| **JsonRpcServer** | ASP.NET Core JSON-RPC 2.0 API server |
| **JsonRpcClient** | WinForms client application |
| **Custom.Business** | Custom business logic assembly |
| **Custom.Contracts** | Shared data contracts |
| **Define** | Configuration and schema files |

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

This sample references the following Bee.NET NuGet packages (v3.6.2):

- Bee.Api.AspNetCore
- Bee.Api.Contracts
- Bee.Business
- Bee.Cache
- Bee.Repository
- Bee.UI.WinForms

## License

MIT License - see [LICENSE](LICENSE) for details.
