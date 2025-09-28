using System.ComponentModel.DataAnnotations;

namespace Avro.Mcp.Orchestrator.Models;

/// <summary>
/// Represents an MCP server that can be orchestrated
/// </summary>
public class McpServerInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public List<string> Capabilities { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public DateTime LastHealthCheck { get; set; } = DateTime.UtcNow;
    public McpServerHealth Health { get; set; } = McpServerHealth.Unknown;
}

/// <summary>
/// Health status of an MCP server
/// </summary>
public enum McpServerHealth
{
    Unknown,
    Healthy,
    Degraded,
    Unhealthy
}

/// <summary>
/// Request to register a new MCP server
/// </summary>
public class RegisterServerRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [Url]
    public string BaseUrl { get; set; } = string.Empty;
    
    public string Version { get; set; } = string.Empty;
    public List<string> Capabilities { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Response after registering a server
/// </summary>
public class RegisterServerResponse
{
    public string Id { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Request to get list of servers
/// </summary>
public class GetServersRequest
{
    public bool? IsActive { get; set; }
    public McpServerHealth? Health { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Response with list of servers
/// </summary>
public class GetServersResponse
{
    public List<McpServerInfo> Servers { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public bool HasMore { get; set; }
}

/// <summary>
/// Request to forward an MCP call to a specific server
/// </summary>
public class ForwardMcpRequest
{
    [Required]
    public string ServerId { get; set; } = string.Empty;
    
    [Required]
    public string Method { get; set; } = string.Empty;
    
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string? TenantId { get; set; }
}

/// <summary>
/// Response from forwarded MCP call
/// </summary>
public class ForwardMcpResponse
{
    public bool Success { get; set; }
    public object? Data { get; set; }
    public string? Error { get; set; }
    public string ServerId { get; set; } = string.Empty;
    public DateTime ResponseTime { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Health check request for a server
/// </summary>
public class HealthCheckRequest
{
    public string ServerId { get; set; } = string.Empty;
}

/// <summary>
/// Health check response
/// </summary>
public class HealthCheckResponse
{
    public string ServerId { get; set; } = string.Empty;
    public McpServerHealth Health { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan ResponseTime { get; set; }
}

/// <summary>
/// Discovery request to find available tools across servers
/// </summary>
public class DiscoverToolsRequest
{
    public string? SearchTerm { get; set; }
    public List<string> ServerIds { get; set; } = new();
    public string? Category { get; set; }
}

/// <summary>
/// Tool information from discovery
/// </summary>
public class ToolInfo
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ServerId { get; set; } = string.Empty;
    public string ServerName { get; set; } = string.Empty;
    public Dictionary<string, object> Schema { get; set; } = new();
    public List<string> Categories { get; set; } = new();
}

/// <summary>
/// Response with discovered tools
/// </summary>
public class DiscoverToolsResponse
{
    public List<ToolInfo> Tools { get; set; } = new();
    public int TotalCount { get; set; }
    public Dictionary<string, int> ServerCounts { get; set; } = new();
}