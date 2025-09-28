# Avro MCP Orchestrator

A Model Context Protocol (MCP) server orchestrator that manages and routes requests to other MCP servers. This service allows you to register multiple MCP servers and orchestrate calls between them, providing centralized management and discovery capabilities.

## Features

- **Server Registration**: Register and manage multiple MCP servers
- **Request Forwarding**: Forward MCP calls to registered servers
- **Tool Discovery**: Discover available tools across all registered servers
- **Health Monitoring**: Monitor health status of registered servers
- **Multi-tenant Support**: Handle tenant-specific routing and isolation
- **RESTful API**: Standard REST endpoints for management operations
- **MCP Protocol**: Native MCP server capabilities with HTTP transport

## Quick Start

### 1. Build and Run

```bash
cd src/Avro.Mcp.Orchestrator
dotnet restore
dotnet build
dotnet run
```

The orchestrator will start on `https://localhost:7234` (or similar).

### 2. Register MCP Servers

Register your MCP servers with the orchestrator:

```bash
curl -X POST "https://localhost:7234/servers/register" \
     -H "Content-Type: application/json" \
     -d '{
       "name": "users-service",
       "description": "User management MCP server",
       "baseUrl": "https://localhost:7001",
       "version": "1.0.0",
       "capabilities": ["tools", "resources"],
       "metadata": {
         "category": "user-management"
       }
     }'
```

### 3. Discover Available Tools

Get all available tools across registered servers:

```bash
curl -X POST "https://localhost:7234/forward/tools/discover" \
     -H "Content-Type: application/json" \
     -d '{
       "searchTerm": "user",
       "category": "management"
     }'
```

### 4. Forward MCP Calls

Forward MCP method calls to registered servers:

```bash
curl -X POST "https://localhost:7234/forward" \
     -H "Content-Type: application/json" \
     -d '{
       "serverId": "your-server-id",
       "method": "get_paged_list_users",
       "parameters": {
         "page": 1,
         "pageSize": 20
       },
       "tenantId": "tenant-123"
     }'
```

## API Endpoints

### Server Management

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/servers/register` | Register a new MCP server |
| GET | `/servers` | Get list of registered servers |
| GET | `/servers/{serverId}` | Get specific server details |
| DELETE | `/servers/{serverId}` | Unregister a server |
| POST | `/servers/{serverId}/health` | Check server health |
| POST | `/servers/health/all` | Check all servers health |

### Request Forwarding

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/forward` | Forward MCP call to registered server |
| POST | `/forward/tools/discover` | Discover tools across servers |

### MCP Protocol

| Endpoint | Description |
|----------|-------------|
| `/api/orchestrator/mcp` | MCP HTTP transport endpoint |

### Utility

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | Service information |
| GET | `/health` | Health check |
| GET | `/swagger` | API documentation |

## MCP Tools

The orchestrator exposes the following MCP tools:

### Server Management Tools

- **`register_mcp_server`**: Register a new MCP server
- **`unregister_mcp_server`**: Unregister an MCP server
- **`get_mcp_servers`**: Get list of registered servers
- **`get_mcp_server`**: Get specific server details
- **`health_check_mcp_server`**: Check server health
- **`health_check_all_servers`**: Check all servers health

### Forwarding Tools

- **`forward_mcp_call`**: Forward MCP method call to a server
- **`discover_mcp_tools`**: Discover available tools across servers

## Configuration

### Multi-tenant Support

The orchestrator supports multi-tenant scenarios through the `ITenantProvider` interface. Tenant ID can be provided via:

- Query parameter: `?tenantId=tenant-123`
- Route parameter: `/{tenantId}/...`
- HTTP header: `X-Tenant-Id: tenant-123`

### Authentication

Authentication is optional by default. To enable authentication for all endpoints, uncomment the following line in `Program.cs`:

```csharp
options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
```

## Example Usage Scenarios

### 1. Microservices Orchestration

Register multiple microservices that expose MCP endpoints:

```json
{
  "name": "user-service",
  "baseUrl": "https://user-service.example.com"
}
```

```json
{
  "name": "order-service", 
  "baseUrl": "https://order-service.example.com"
}
```

### 2. Tool Discovery

Find all user-related tools across services:

```json
{
  "searchTerm": "user",
  "serverIds": ["user-service-id", "auth-service-id"]
}
```

### 3. Cross-Service Operations

Forward requests with tenant context:

```json
{
  "serverId": "user-service-id",
  "method": "get_user_profile",
  "parameters": {"userId": "123"},
  "tenantId": "company-abc"
}
```

## Development

### Project Structure

```
src/Avro.Mcp.Orchestrator/
├── Endpoints/          # API endpoints and MCP tools
├── Infrastructure/     # Cross-cutting concerns
├── Models/            # Data models and DTOs
├── Services/          # Business logic services
├── Validators/        # Request validation
└── Program.cs         # Application startup
```

### Adding New Features

1. Define models in `Models/Models.cs`
2. Add validation in `Validators/Validators.cs`
3. Implement business logic in `Services/`
4. Create endpoints in `Endpoints/`
5. Register services in `Program.cs`

### Testing

Use the MCP Inspector for testing MCP functionality:
- https://modelcontextprotocol.io/legacy/tools/inspector

Connect to: `http://localhost:5234/api/orchestrator/mcp`

### Health Monitoring

The orchestrator automatically monitors registered server health. Servers are checked:
- On registration
- On explicit health check requests
- During forwarding operations (if server appears unhealthy)

## Dependencies

- .NET 8.0
- ModelContextProtocol (>= 0.5.0)
- ModelContextProtocol.AspNetCore (>= 0.5.0)
- FluentValidation (>= 11.9.2)
- Microsoft.AspNetCore.OpenApi
- Swashbuckle.AspNetCore

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make changes following the existing patterns
4. Add tests for new functionality
5. Submit a pull request

## License

This project is licensed under the MIT License.