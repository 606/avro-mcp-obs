using Avro.Mcp.Orchestrator.Models;
using System.Collections.Concurrent;

namespace Avro.Mcp.Orchestrator.Services;

/// <summary>
/// Interface for managing MCP servers
/// </summary>
public interface IMcpServerManager
{
    Task<RegisterServerResponse> RegisterServerAsync(RegisterServerRequest request);
    Task<bool> UnregisterServerAsync(string serverId);
    Task<GetServersResponse> GetServersAsync(GetServersRequest request);
    Task<McpServerInfo?> GetServerAsync(string serverId);
    Task<HealthCheckResponse> HealthCheckAsync(string serverId);
    Task<List<HealthCheckResponse>> HealthCheckAllAsync();
    Task<bool> UpdateServerHealthAsync(string serverId, McpServerHealth health, string message = "");
}

/// <summary>
/// Service for managing MCP servers in memory
/// </summary>
public class InMemoryMcpServerManager : IMcpServerManager
{
    private readonly ConcurrentDictionary<string, McpServerInfo> _servers = new();
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<InMemoryMcpServerManager> _logger;

    public InMemoryMcpServerManager(IHttpClientFactory httpClientFactory, ILogger<InMemoryMcpServerManager> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public Task<RegisterServerResponse> RegisterServerAsync(RegisterServerRequest request)
    {
        var serverId = Guid.NewGuid().ToString();
        var server = new McpServerInfo
        {
            Id = serverId,
            Name = request.Name,
            Description = request.Description,
            BaseUrl = request.BaseUrl.TrimEnd('/'),
            Version = request.Version,
            Capabilities = request.Capabilities,
            Metadata = request.Metadata,
            RegisteredAt = DateTime.UtcNow,
            LastHealthCheck = DateTime.UtcNow,
            Health = McpServerHealth.Unknown
        };

        _servers.TryAdd(serverId, server);

        _logger.LogInformation("Registered MCP server {ServerId} ({ServerName}) at {BaseUrl}", 
            serverId, request.Name, request.BaseUrl);

        return Task.FromResult(new RegisterServerResponse
        {
            Id = serverId,
            Message = $"Server '{request.Name}' registered successfully"
        });
    }

    public Task<bool> UnregisterServerAsync(string serverId)
    {
        var result = _servers.TryRemove(serverId, out var server);
        
        if (result && server != null)
        {
            _logger.LogInformation("Unregistered MCP server {ServerId} ({ServerName})", 
                serverId, server.Name);
        }

        return Task.FromResult(result);
    }

    public Task<GetServersResponse> GetServersAsync(GetServersRequest request)
    {
        var servers = _servers.Values.AsQueryable();

        // Apply filters
        if (request.IsActive.HasValue)
        {
            servers = servers.Where(s => s.IsActive == request.IsActive.Value);
        }

        if (request.Health.HasValue)
        {
            servers = servers.Where(s => s.Health == request.Health.Value);
        }

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLowerInvariant();
            servers = servers.Where(s => 
                s.Name.ToLowerInvariant().Contains(searchTerm) ||
                s.Description.ToLowerInvariant().Contains(searchTerm));
        }

        var totalCount = servers.Count();
        var pagedServers = servers
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return Task.FromResult(new GetServersResponse
        {
            Servers = pagedServers,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            HasMore = (request.Page * request.PageSize) < totalCount
        });
    }

    public Task<McpServerInfo?> GetServerAsync(string serverId)
    {
        _servers.TryGetValue(serverId, out var server);
        return Task.FromResult(server);
    }

    public async Task<HealthCheckResponse> HealthCheckAsync(string serverId)
    {
        var server = await GetServerAsync(serverId);
        if (server == null)
        {
            return new HealthCheckResponse
            {
                ServerId = serverId,
                Health = McpServerHealth.Unhealthy,
                Message = "Server not found",
                CheckedAt = DateTime.UtcNow
            };
        }

        var startTime = DateTime.UtcNow;
        var health = McpServerHealth.Unhealthy;
        var message = "Unknown error";

        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            // Try to call a health endpoint or the MCP base endpoint
            var healthUrl = $"{server.BaseUrl}/health";
            var response = await httpClient.GetAsync(healthUrl);

            if (response.IsSuccessStatusCode)
            {
                health = McpServerHealth.Healthy;
                message = "Health check successful";
            }
            else
            {
                health = McpServerHealth.Degraded;
                message = $"Health endpoint returned {response.StatusCode}";
            }
        }
        catch (TaskCanceledException)
        {
            health = McpServerHealth.Unhealthy;
            message = "Health check timeout";
        }
        catch (Exception ex)
        {
            health = McpServerHealth.Unhealthy;
            message = $"Health check failed: {ex.Message}";
        }

        var responseTime = DateTime.UtcNow - startTime;

        // Update server health
        await UpdateServerHealthAsync(serverId, health, message);

        return new HealthCheckResponse
        {
            ServerId = serverId,
            Health = health,
            Message = message,
            CheckedAt = DateTime.UtcNow,
            ResponseTime = responseTime
        };
    }

    public async Task<List<HealthCheckResponse>> HealthCheckAllAsync()
    {
        var tasks = _servers.Values.Select(server => HealthCheckAsync(server.Id));
        var results = await Task.WhenAll(tasks);
        return results.ToList();
    }

    public Task<bool> UpdateServerHealthAsync(string serverId, McpServerHealth health, string message = "")
    {
        if (_servers.TryGetValue(serverId, out var server))
        {
            server.Health = health;
            server.LastHealthCheck = DateTime.UtcNow;
            
            _logger.LogDebug("Updated health for server {ServerId}: {Health} - {Message}", 
                serverId, health, message);
            
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }
}