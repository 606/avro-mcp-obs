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

# Avro Git MCP Server

The Git MCP Server provides comprehensive Git automation capabilities through the Model Context Protocol. This server enables AI assistants and other clients to perform Git operations programmatically.

## Git MCP Features

- **Repository Management**: Clone, initialize, and manage Git repositories
- **Commit Operations**: Stage files, create commits with full metadata
- **Branch Management**: Create, switch, merge, and delete branches
- **Remote Operations**: Push and pull changes to/from remote repositories
- **Status & History**: Get repository status, commit history, and branch information
- **Multi-tenant Support**: Isolate Git operations by tenant

## Git MCP Quick Start

### 1. Run Git MCP Server

```bash
cd src/Avro.Mcp.Git
dotnet restore
dotnet build
dotnet run
```

The Git MCP server will start on `https://localhost:7235` (or similar).

### 2. Register with Orchestrator

Register the Git server with the orchestrator:

```bash
curl -X POST "https://localhost:7234/servers/register" \
     -H "Content-Type: application/json" \
     -d '{
       "name": "git-server",
       "description": "Git operations MCP server",
       "baseUrl": "https://localhost:7235",
       "version": "1.0.0",
       "capabilities": ["git", "repository", "version-control"],
       "metadata": {
         "category": "git-automation"
       }
     }'
```

### 3. Git Operations Examples

#### Clone Repository
```bash
curl -X POST "https://localhost:7235/git/repository/clone" \
     -H "Content-Type: application/json" \
     -d '{
       "remoteUrl": "https://github.com/user/repo.git",
       "localPath": "/path/to/local/repo",
       "branch": "main",
       "username": "your-username",
       "password": "your-token"
     }'
```

#### Create Commit
```bash
curl -X POST "https://localhost:7235/git/commits" \
     -H "Content-Type: application/json" \
     -d '{
       "repositoryPath": "/path/to/repo",
       "message": "Add new feature",
       "authorName": "Developer",
       "authorEmail": "dev@example.com",
       "addAll": true
     }'
```

#### Create Branch
```bash
curl -X POST "https://localhost:7235/git/branches" \
     -H "Content-Type: application/json" \
     -d '{
       "repositoryPath": "/path/to/repo",
       "branchName": "feature/new-feature",
       "checkout": true
     }'
```

## Git MCP Tools

### Repository Tools
- **`git_clone_repository`**: Clone remote repository
- **`git_init_repository`**: Initialize new repository
- **`git_get_repository_info`**: Get repository metadata
- **`git_get_status`**: Get working directory status

### Commit Tools
- **`git_commit_changes`**: Stage and commit changes
- **`git_get_commits`**: Get commit history with filtering

### Branch Tools
- **`git_create_branch`**: Create new branch
- **`git_get_branches`**: List all branches
- **`git_checkout_branch`**: Switch branches
- **`git_delete_branch`**: Remove branch
- **`git_merge_branch`**: Merge branches

### Remote Tools
- **`git_push_changes`**: Push to remote repository
- **`git_pull_changes`**: Pull from remote repository

## Git MCP API Endpoints

### Repository Operations

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/git/repository/clone` | Clone a repository |
| POST | `/git/repository/init` | Initialize repository |
| GET | `/git/repository/info` | Get repository info |
| GET | `/git/repository/status` | Get repository status |

### Commit Operations

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/git/commits` | Create a commit |
| GET | `/git/commits` | Get commit history |

### Branch Operations

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/git/branches` | Create a branch |
| GET | `/git/branches` | List branches |
| POST | `/git/branches/checkout` | Checkout branch |
| DELETE | `/git/branches/{name}` | Delete branch |
| POST | `/git/branches/merge` | Merge branches |

### Remote Operations

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/git/remote/push` | Push changes |
| POST | `/git/remote/pull` | Pull changes |

## Git Workflow Examples

### 1. Complete Feature Development

```bash
# 1. Clone repository
curl -X POST "localhost:7235/git/repository/clone" \
  -d '{"remoteUrl": "https://github.com/user/repo.git", "localPath": "/work/repo"}'

# 2. Create feature branch
curl -X POST "localhost:7235/git/branches" \
  -d '{"repositoryPath": "/work/repo", "branchName": "feature/auth", "checkout": true}'

# 3. Make changes and commit
curl -X POST "localhost:7235/git/commits" \
  -d '{"repositoryPath": "/work/repo", "message": "Add authentication", "authorName": "Dev", "authorEmail": "dev@example.com", "addAll": true}'

# 4. Push changes
curl -X POST "localhost:7235/git/remote/push" \
  -d '{"repositoryPath": "/work/repo", "branch": "feature/auth"}'
```

### 2. Release Management

```bash
# 1. Create release branch
curl -X POST "localhost:7235/git/branches" \
  -d '{"repositoryPath": "/work/repo", "branchName": "release/v1.0", "checkout": true}'

# 2. Merge feature branches
curl -X POST "localhost:7235/git/branches/merge" \
  -d '{"repositoryPath": "/work/repo", "sourceBranch": "feature/auth", "committerName": "Release Manager", "committerEmail": "rm@example.com"}'

# 3. Tag and push
curl -X POST "localhost:7235/git/commits" \
  -d '{"repositoryPath": "/work/repo", "message": "Release v1.0", "authorName": "Release Manager", "authorEmail": "rm@example.com"}'

curl -X POST "localhost:7235/git/remote/push" \
  -d '{"repositoryPath": "/work/repo", "branch": "release/v1.0"}'
```

## Using with Orchestrator

Once registered with the orchestrator, you can use Git operations through MCP calls:

```bash
# Forward Git operation through orchestrator
curl -X POST "https://localhost:7234/forward" \
     -H "Content-Type: application/json" \
     -d '{
       "serverId": "git-server-id",
       "method": "git_clone_repository", 
       "parameters": {
         "remoteUrl": "https://github.com/user/repo.git",
         "localPath": "/path/to/repo"
       },
       "tenantId": "team-alpha"
     }'
```

## Configuration

### Git Server Settings

```json
{
  "Git": {
    "ServiceName": "git",
    "DefaultTimeout": "00:05:00",
    "MaxRepositorySize": "1073741824"
  }
}
```

### Authentication

The Git server supports the same authentication patterns as the orchestrator:
- Optional JWT authentication
- Multi-tenant isolation
- Request validation

## Testing Git MCP Server Locally

### 1. Prerequisites

Make sure you have:
- .NET 8.0 SDK installed
- Git installed on your system
- A test repository (local or remote)

### 2. Start the Git MCP Server

```bash
# Navigate to Git MCP project
cd src/Avro.Mcp.Git

# Restore dependencies
dotnet restore

# Run the server
dotnet run
```

The server will start on `https://localhost:5001` or similar (check console output).

### 3. Test with MCP Inspector

#### Option A: Use MCP Inspector Tool
1. Open https://modelcontextprotocol.io/legacy/tools/inspector
2. Connect to: `https://localhost:5001/api/git/mcp`
3. Explore available tools and test them interactively

#### Option B: Use curl commands (detailed examples below)

### 4. Local Testing Scenarios

#### Test 1: Repository Information

```bash
# Create a test repository first
mkdir /tmp/test-repo
cd /tmp/test-repo
git init
echo "# Test Repo" > README.md
git add README.md
git config user.name "Test User"
git config user.email "test@example.com"
git commit -m "Initial commit"

# Test getting repository info
curl -X GET "https://localhost:5001/git/repository/info?repositoryPath=/tmp/test-repo" \
     -H "Accept: application/json" \
     -k
```

#### Test 2: Repository Status

```bash
# Make some changes to test status
echo "Some changes" >> /tmp/test-repo/README.md
echo "New file" > /tmp/test-repo/newfile.txt

# Check status via MCP
curl -X GET "https://localhost:5001/git/repository/status?repositoryPath=/tmp/test-repo" \
     -H "Accept: application/json" \
     -k
```

#### Test 3: Create and Commit Changes

```bash
# Commit changes via MCP
curl -X POST "https://localhost:5001/git/commits" \
     -H "Content-Type: application/json" \
     -H "Accept: application/json" \
     -k \
     -d '{
       "repositoryPath": "/tmp/test-repo",
       "message": "Add new content via MCP",
       "authorName": "MCP Test",
       "authorEmail": "mcp@test.com",
       "addAll": true
     }'
```

#### Test 4: Branch Operations

```bash
# Create a new branch
curl -X POST "https://localhost:5001/git/branches" \
     -H "Content-Type: application/json" \
     -k \
     -d '{
       "repositoryPath": "/tmp/test-repo",
       "branchName": "feature/test-branch",
       "checkout": true
     }'

# List all branches
curl -X GET "https://localhost:5001/git/branches?repositoryPath=/tmp/test-repo" \
     -H "Accept: application/json" \
     -k

# Switch back to main
curl -X POST "https://localhost:5001/git/branches/checkout" \
     -H "Content-Type: application/json" \
     -k \
     -d '{
       "repositoryPath": "/tmp/test-repo",
       "branchName": "main"
     }'
```

#### Test 5: Clone Repository

```bash
# Clone a public repository
curl -X POST "https://localhost:5001/git/repository/clone" \
     -H "Content-Type: application/json" \
     -k \
     -d '{
       "remoteUrl": "https://github.com/octocat/Hello-World.git",
       "localPath": "/tmp/cloned-repo"
     }'
```

#### Test 6: Get Commit History

```bash
# Get recent commits
curl -X GET "https://localhost:5001/git/commits?repositoryPath=/tmp/test-repo&take=5" \
     -H "Accept: application/json" \
     -k
```

### 5. Testing with Orchestrator

#### Step 1: Start Both Services

```bash
# Terminal 1: Start Git MCP Server
cd src/Avro.Mcp.Git
dotnet run

# Terminal 2: Start Orchestrator
cd src/Avro.Mcp.Orchestrator  
dotnet run
```

#### Step 2: Register Git Server with Orchestrator

```bash
# Register the Git server
curl -X POST "https://localhost:7234/servers/register" \
     -H "Content-Type: application/json" \
     -d '{
       "name": "git-server",
       "description": "Local Git MCP server for testing",
       "baseUrl": "https://localhost:5001",
       "version": "1.0.0",
       "capabilities": ["git", "repository", "version-control"],
       "metadata": {
         "category": "git-automation"
       }
     }'
```

#### Step 3: Test Git Operations via Orchestrator

```bash
# Get the server ID from registration response, then test forwarding
curl -X POST "https://localhost:7234/forward" \
     -H "Content-Type: application/json" \
     -d '{
       "serverId": "YOUR_SERVER_ID_HERE",
       "method": "git_get_repository_info",
       "parameters": {
         "repositoryPath": "/tmp/test-repo"
       }
     }'
```

#### Step 4: Discover Git Tools via Orchestrator

```bash
# Discover all Git tools
curl -X POST "https://localhost:7234/forward/tools/discover" \
     -H "Content-Type: application/json" \
     -d '{
       "searchTerm": "git",
       "serverIds": ["YOUR_SERVER_ID_HERE"]
     }'
```

### 6. Testing MCP Protocol Directly

You can also test the MCP protocol directly using the MCP JSON-RPC format:

```bash
# Test tools/list MCP method
curl -X POST "https://localhost:5001/api/git/mcp" \
     -H "Content-Type: application/json" \
     -k \
     -d '{
       "jsonrpc": "2.0",
       "id": "test-1",
       "method": "tools/list"
     }'

# Test tools/call MCP method
curl -X POST "https://localhost:5001/api/git/mcp" \
     -H "Content-Type: application/json" \
     -k \
     -d '{
       "jsonrpc": "2.0",
       "id": "test-2", 
       "method": "tools/call",
       "params": {
         "name": "git_get_repository_info",
         "arguments": {
           "repositoryPath": "/tmp/test-repo"
         }
       }
     }'
```

### 7. Debugging Tips

#### Enable Detailed Logging
Add to `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Avro.Mcp.Git": "Trace",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
```

#### Check Server Health
```bash
curl -X GET "https://localhost:5001/health" -k
```

#### View API Documentation
Open `https://localhost:5001/swagger` in your browser to explore the API interactively.

### 8. Common Issues and Solutions

**Issue**: SSL Certificate errors
**Solution**: Use `-k` flag with curl or configure proper certificates

**Issue**: Repository not found
**Solution**: Ensure the repository path exists and is a valid Git repository

**Issue**: Permission denied
**Solution**: Check file permissions on the repository directory

**Issue**: LibGit2Sharp errors
**Solution**: Ensure Git is installed and the repository isn't corrupted

### 9. Automated Test Script

Create a test script `test-git-mcp.sh`:

```bash
#!/bin/bash
set -e

BASE_URL="https://localhost:5001"
TEST_REPO="/tmp/mcp-test-repo"

echo "🧪 Testing Git MCP Server..."

# Cleanup previous test
rm -rf $TEST_REPO

# Initialize test repository
echo "📁 Setting up test repository..."
curl -X POST "$BASE_URL/git/repository/init" \
     -H "Content-Type: application/json" \
     -k -s \
     -d "{\"localPath\": \"$TEST_REPO\"}"

# Check repository info
echo "ℹ️  Getting repository info..."
curl -X GET "$BASE_URL/git/repository/info?repositoryPath=$TEST_REPO" \
     -H "Accept: application/json" \
     -k -s | jq .

# Create initial commit
echo "📝 Creating initial commit..."
echo "# Test Repository" > "$TEST_REPO/README.md"
curl -X POST "$BASE_URL/git/commits" \
     -H "Content-Type: application/json" \
     -k -s \
     -d "{
       \"repositoryPath\": \"$TEST_REPO\",
       \"message\": \"Initial commit\",
       \"authorName\": \"Test User\",
       \"authorEmail\": \"test@example.com\",
       \"addAll\": true
     }" | jq .

echo "✅ Git MCP Server tests completed!"
```

Run with: `chmod +x test-git-mcp.sh && ./test-git-mcp.sh`

## Dependencies

- .NET 8.0
- LibGit2Sharp (>= 0.30.0) - Native Git operations
- ModelContextProtocol packages
- FluentValidation

## License

This project is licensed under the MIT License.