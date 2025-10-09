# HolidayShow Copilot Instructions

## Architecture Overview

This is a distributed IoT holiday light/fire show system with a TCP-based client-server architecture:

- **HolidayShowServer.Core**: Console TCP server that orchestrates GPIO control and audio file distribution
- **HolidayShowClient.Core**: IoT client (Raspberry Pi/Windows IoT) that connects via TCP and controls hardware
- **HolidayShowWeb**: ASP.NET Core web app with React frontend for show management
- **HolidayShow.Data.Core**: Entity Framework Core data layer with SQL Server backend
- **HolidayShowLib.Core/Shared.Core**: Protocol definitions and shared utilities

## Key Architectural Patterns

### TCP Protocol Communication
- Custom message protocol defined in `ProtocolMessage.cs` with enum-based message types (`MessageTypeIdEnum`)
- Key message constants: `PINID`, `DEVID`, `DURATION`, `PINON`, `AUDIOFILE`, `FILEDOWNLOAD`
- Server maintains client connections in `RemoteClient` objects, handles keep-alive and GPIO commands
- Async TCP handling in `TcpServer.cs` with event-driven client management

### Data Layer Conventions
- Entity models use partial classes (e.g., `AudioOptions.Partial.cs`, `DevicePatterns.Partial.cs`)
- EF Context: `EfHolidayContext` with lazy loading enabled
- Models follow naming: `Devices`, `DeviceIoPorts`, `DevicePatterns`, `Sets`, `SetSequences`
- Connection string key: `"EfHolidayContext"` in config files

### Service Resolution Pattern
- Custom DI abstraction via `IResolverService` interface in `HolidayShowLibShared.Core`
- Used throughout for type resolution instead of direct DI container access

## Essential Development Workflows

### Database Operations
```bash
# Create EF migration
npm run ef-migrate-db
# Or directly: ./scripts/dotnet_ef_migration_create.sh ./HolidayShow.Data.Core EfHolidayContext
```

### Docker Development
```bash
# Build all containers
npm run build-all
# Build specific architecture 
npm run build-x64
# Run with shell access for debugging
npm run docker-run-shell
```

### Version Management
- NPM scripts handle versioning: `client-patch`, `web-minor`, `server-patch`
- Uses semantic versioning with component-specific bumps

## Configuration Patterns

### Connection Strings
- Server: Uses `ConfigurationManager.ConnectionStrings["EfHolidayContext"]`
- Web: Configured via `appsettings.json` with EF in `Startup.cs`
- Default development: SQL Server on port 1401 with dev/dev123 credentials

### Cross-Platform Support
- Targets both Windows IoT and Linux/Docker deployments
- Conditional compilation directives used for platform-specific GPIO access
- Serilog logging to `/var/log/HolidayShowServer/` in containers

## Important File Locations

### Critical Config Files
- `HolidayShowServer.Core/Program.cs`: Main server logic, client management, set execution
- `HolidayShowClient.Core/Program.cs`: IoT client TCP connection and GPIO control
- `HolidayShow.Data.Core/EfHolidayContext.cs`: EF model configuration
- `package.json`: Build script orchestration

### Protocol Definitions
- `HolidayShowLib.Core/MessageTypeId.cs`: Core message types enum
- `HolidayShowLib.Core/ProtocolMessage.cs`: Message structure and constants

### Web Interface
- `HolidayShowWeb/Startup.cs`: ASP.NET Core configuration with React SPA
- Web controllers in `HolidayShowWeb/Controllers/`
- React frontend in `HolidayShowWeb/ClientApp/`

### React Frontend Patterns
- **MobX State Management**: Single `appStore.js` with `@observable`, `@action`, `@computed` decorators
- **Service Layer**: Dedicated services for each domain (SetServices, DeviceServices, AudioServices, etc.)
- **Component Structure**: `@inject("appStore")` and `@observer` decorators for MobX integration
- **UI Framework**: Material-UI with Semantic UI React components
- **Data Querying**: LINQ-ES5 for client-side data manipulation and filtering
- **Global Patterns**: Centralized busy state management with delayed loading indicators

## Common Gotchas

- **GPIO Access**: Platform-specific - check preprocessor directives in client code
- **Timer Management**: Server uses `ConcurrentDictionary<Timer, bool>` for queued operations
- **File Downloads**: Audio files downloaded on-demand from server to clients via TCP
- **Connection Handling**: TCP clients auto-reconnect, server maintains connection state
- **Database**: Auto-applies migrations on server startup
- **Configuration**: Multiple project types use different config patterns (app.config vs appsettings.json)
- **MobX Decorators**: Requires `@babel/plugin-proposal-decorators` for `@observable/@action` syntax
- **React Build**: Uses CRACO for custom configuration, copies build to `../wwwroot/react/`
- **LINQ Client-Side**: Heavy use of `linq-es5` for data transformations in React components

## Project-Specific Notes

- This is described as "get-it-done software" - prioritize functionality over enterprise patterns
- Originally evolved from 2013 Mono/Raspberry Pi project to current .NET Core/Docker stack
- Heavy use of timers for show sequencing and GPIO timing control
- Custom protocol over TCP rather than HTTP/REST for real-time GPIO control performance