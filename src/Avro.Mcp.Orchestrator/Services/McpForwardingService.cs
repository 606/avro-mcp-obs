using Avro.Mcp.Orchestrator.Models;
using System.Text;
using System.Text.Json;

namespace Avro.Mcp.Orchestrator.Services;

/// <summary>
/// Interface for forwarding MCP calls to registered servers
/// </summary>
public interface IMcpForwardingService
{
    Task<ForwardMcpResponse> ForwardCallAsync(ForwardMcpRequest request);
    Task<DiscoverToolsResponse> DiscoverToolsAsync(DiscoverToolsRequest request);
}

/// <summary>
/// Service for forwarding MCP calls to registered servers
/// </summary>
public class McpForwardingService : IMcpForwardingService
{
    private readonly IMcpServerManager _serverManager;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<McpForwardingService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public McpForwardingService(
        IMcpServerManager serverManager, 
        IHttpClientFactory httpClientFactory, 
        ILogger<McpForwardingService> logger)
    {
        _serverManager = serverManager;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<ForwardMcpResponse> ForwardCallAsync(ForwardMcpRequest request)
    {
        var server = await _serverManager.GetServerAsync(request.ServerId);
        if (server == null)
        {
            return new ForwardMcpResponse
            {
                Success = false,
                Error = $"Server '{request.ServerId}' not found",
                ServerId = request.ServerId
            };
        }

        if (!server.IsActive || server.Health == McpServerHealth.Unhealthy)
        {
            return new ForwardMcpResponse
            {
                Success = false,
                Error = $"Server '{request.ServerId}' is not available (Active: {server.IsActive}, Health: {server.Health})",
                ServerId = request.ServerId
            };
        }

        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            // Build the target URL
            var targetUrl = $"{server.BaseUrl}/api/{server.Name}/mcp";
            
            // Add tenant ID to query if provided
            if (!string.IsNullOrEmpty(request.TenantId))
            {
                targetUrl += $"?tenantId={Uri.EscapeDataString(request.TenantId)}";
            }

            // Create MCP request payload
            var mcpRequest = new
            {
                jsonrpc = "2.0",
                id = Guid.NewGuid().ToString(),
                method = request.Method,
                @params = request.Parameters
            };

            var jsonContent = JsonSerializer.Serialize(mcpRequest, _jsonOptions);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _logger.LogDebug("Forwarding MCP call to {TargetUrl}: {Method}", targetUrl, request.Method);

            var response = await httpClient.PostAsync(targetUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var mcpResponse = JsonSerializer.Deserialize<JsonElement>(responseContent, _jsonOptions);
                
                // Check if it's an MCP error response
                if (mcpResponse.TryGetProperty("error", out var errorElement))
                {
                    return new ForwardMcpResponse
                    {
                        Success = false,
                        Error = errorElement.GetString() ?? "Unknown MCP error",
                        ServerId = request.ServerId,
                        Data = mcpResponse
                    };
                }

                // Extract the result if available
                object? resultData = null;
                if (mcpResponse.TryGetProperty("result", out var resultElement))
                {
                    resultData = JsonSerializer.Deserialize<object>(resultElement.GetRawText());
                }
                else
                {
                    resultData = JsonSerializer.Deserialize<object>(responseContent);
                }

                return new ForwardMcpResponse
                {
                    Success = true,
                    Data = resultData,
                    ServerId = request.ServerId
                };
            }
            else
            {
                return new ForwardMcpResponse
                {
                    Success = false,
                    Error = $"HTTP {response.StatusCode}: {responseContent}",
                    ServerId = request.ServerId
                };
            }
        }
        catch (TaskCanceledException)
        {
            return new ForwardMcpResponse
            {
                Success = false,
                Error = "Request timeout",
                ServerId = request.ServerId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error forwarding MCP call to server {ServerId}", request.ServerId);
            
            return new ForwardMcpResponse
            {
                Success = false,
                Error = $"Forwarding failed: {ex.Message}",
                ServerId = request.ServerId
            };
        }
    }

    public async Task<DiscoverToolsResponse> DiscoverToolsAsync(DiscoverToolsRequest request)
    {
        var serversRequest = new GetServersRequest
        {
            IsActive = true
        };

        var serversResponse = await _serverManager.GetServersAsync(serversRequest);
        var servers = serversResponse.Servers;

        // Filter by specific server IDs if requested
        if (request.ServerIds.Any())
        {
            servers = servers.Where(s => request.ServerIds.Contains(s.Id)).ToList();
        }

        var allTools = new List<ToolInfo>();
        var serverCounts = new Dictionary<string, int>();

        // Discover tools from each server
        foreach (var server in servers)
        {
            try
            {
                var tools = await DiscoverServerToolsAsync(server);
                
                // Apply search filter if specified
                if (!string.IsNullOrEmpty(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.ToLowerInvariant();
                    tools = tools.Where(t => 
                        t.Name.ToLowerInvariant().Contains(searchTerm) ||
                        t.Description.ToLowerInvariant().Contains(searchTerm)
                    ).ToList();
                }

                // Apply category filter if specified
                if (!string.IsNullOrEmpty(request.Category))
                {
                    tools = tools.Where(t => 
                        t.Categories.Any(c => c.Equals(request.Category, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }

                allTools.AddRange(tools);
                serverCounts[server.Id] = tools.Count;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to discover tools from server {ServerId} ({ServerName})", 
                    server.Id, server.Name);
                serverCounts[server.Id] = 0;
            }
        }

        return new DiscoverToolsResponse
        {
            Tools = allTools,
            TotalCount = allTools.Count,
            ServerCounts = serverCounts
        };
    }

    private async Task<List<ToolInfo>> DiscoverServerToolsAsync(McpServerInfo server)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            // Try to call the tools/list endpoint
            var toolsUrl = $"{server.BaseUrl}/api/{server.Name}/mcp";
            
            var mcpRequest = new
            {
                jsonrpc = "2.0",
                id = Guid.NewGuid().ToString(),
                method = "tools/list"
            };

            var jsonContent = JsonSerializer.Serialize(mcpRequest, _jsonOptions);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(toolsUrl, content);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to discover tools from {ServerName}: HTTP {StatusCode}", 
                    server.Name, response.StatusCode);
                return new List<ToolInfo>();
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var mcpResponse = JsonSerializer.Deserialize<JsonElement>(responseContent, _jsonOptions);

            var tools = new List<ToolInfo>();

            if (mcpResponse.TryGetProperty("result", out var resultElement) &&
                resultElement.TryGetProperty("tools", out var toolsElement))
            {
                foreach (var toolElement in toolsElement.EnumerateArray())
                {
                    var tool = new ToolInfo
                    {
                        ServerId = server.Id,
                        ServerName = server.Name
                    };

                    if (toolElement.TryGetProperty("name", out var nameElement))
                    {
                        tool.Name = nameElement.GetString() ?? "";
                    }

                    if (toolElement.TryGetProperty("description", out var descElement))
                    {
                        tool.Description = descElement.GetString() ?? "";
                    }

                    if (toolElement.TryGetProperty("inputSchema", out var schemaElement))
                    {
                        tool.Schema = JsonSerializer.Deserialize<Dictionary<string, object>>(schemaElement.GetRawText()) ?? new();
                    }

                    // Extract categories from metadata or schema
                    if (toolElement.TryGetProperty("category", out var categoryElement))
                    {
                        var category = categoryElement.GetString();
                        if (!string.IsNullOrEmpty(category))
                        {
                            tool.Categories.Add(category);
                        }
                    }

                    tools.Add(tool);
                }
            }

            return tools;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering tools from server {ServerId} ({ServerName})", 
                server.Id, server.Name);
            return new List<ToolInfo>();
        }
    }
}