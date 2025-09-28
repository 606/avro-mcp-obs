using Avro.Mcp.Orchestrator.Models;
using Avro.Mcp.Orchestrator.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ModelContextProtocol.AspNetCore;

namespace Avro.Mcp.Orchestrator.Endpoints;

/// <summary>
/// MCP tools for server management and orchestration
/// </summary>
[McpServerToolType]
public static class ServerManagementEndpoints
{
    /// <summary>
    /// Map server management endpoints
    /// </summary>
    public static IEndpointRouteBuilder MapServerManagement(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/servers")
            .WithTags("Server Management")
            .WithOpenApi();

        group.MapPost("/register", RegisterServer)
            .WithSummary("Register a new MCP server")
            .Produces<RegisterServerResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapDelete("/{serverId}", UnregisterServer)
            .WithSummary("Unregister an MCP server")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", GetServers)
            .WithSummary("Get list of registered MCP servers")
            .Produces<GetServersResponse>(StatusCodes.Status200OK);

        group.MapGet("/{serverId}", GetServer)
            .WithSummary("Get details of a specific MCP server")
            .Produces<McpServerInfo>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{serverId}/health", HealthCheck)
            .WithSummary("Perform health check on a specific server")
            .Produces<HealthCheckResponse>(StatusCodes.Status200OK);

        group.MapPost("/health/all", HealthCheckAll)
            .WithSummary("Perform health check on all servers")
            .Produces<List<HealthCheckResponse>>(StatusCodes.Status200OK);

        return app;
    }

    /// <summary>
    /// Register a new MCP server for orchestration
    /// </summary>
    [McpServerTool(Name = "register_mcp_server")]
    [Description("Register a new MCP server that can be orchestrated by this service")]
    private static async Task<IResult> RegisterServer(
        [FromBody] RegisterServerRequest request,
        IMcpServerManager serverManager,
        IValidator<RegisterServerRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var response = await serverManager.RegisterServerAsync(request);
        return Results.Ok(response);
    }

    /// <summary>
    /// Unregister an MCP server
    /// </summary>
    [McpServerTool(Name = "unregister_mcp_server")]
    [Description("Unregister an MCP server from orchestration")]
    private static async Task<IResult> UnregisterServer(
        string serverId,
        IMcpServerManager serverManager)
    {
        var success = await serverManager.UnregisterServerAsync(serverId);
        return success ? Results.NoContent() : Results.NotFound();
    }

    /// <summary>
    /// Get list of registered MCP servers
    /// </summary>
    [McpServerTool(Name = "get_mcp_servers")]
    [Description("Get a list of all registered MCP servers with filtering and pagination")]
    private static async Task<IResult> GetServers(
        [AsParameters] GetServersRequest request,
        IMcpServerManager serverManager,
        IValidator<GetServersRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var response = await serverManager.GetServersAsync(request);
        return Results.Ok(response);
    }

    /// <summary>
    /// Get details of a specific MCP server
    /// </summary>
    [McpServerTool(Name = "get_mcp_server")]
    [Description("Get detailed information about a specific registered MCP server")]
    private static async Task<IResult> GetServer(
        string serverId,
        IMcpServerManager serverManager)
    {
        var server = await serverManager.GetServerAsync(serverId);
        return server != null ? Results.Ok(server) : Results.NotFound();
    }

    /// <summary>
    /// Perform health check on a specific server
    /// </summary>
    [McpServerTool(Name = "health_check_mcp_server")]
    [Description("Check the health status of a specific MCP server")]
    private static async Task<IResult> HealthCheck(
        string serverId,
        IMcpServerManager serverManager)
    {
        var response = await serverManager.HealthCheckAsync(serverId);
        return Results.Ok(response);
    }

    /// <summary>
    /// Perform health check on all servers
    /// </summary>
    [McpServerTool(Name = "health_check_all_servers")]
    [Description("Check the health status of all registered MCP servers")]
    private static async Task<IResult> HealthCheckAll(
        IMcpServerManager serverManager)
    {
        var responses = await serverManager.HealthCheckAllAsync();
        return Results.Ok(responses);
    }
}

/// <summary>
/// MCP tools for request forwarding and orchestration
/// </summary>
[McpServerToolType]
public static class ForwardingEndpoints
{
    /// <summary>
    /// Map forwarding endpoints
    /// </summary>
    public static IEndpointRouteBuilder MapForwarding(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/forward")
            .WithTags("Request Forwarding")
            .WithOpenApi();

        group.MapPost("/", ForwardMcpCall)
            .WithSummary("Forward an MCP call to a registered server")
            .Produces<ForwardMcpResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/tools/discover", DiscoverTools)
            .WithSummary("Discover available tools across registered servers")
            .Produces<DiscoverToolsResponse>(StatusCodes.Status200OK);

        return app;
    }

    /// <summary>
    /// Forward an MCP call to a registered server
    /// </summary>
    [McpServerTool(Name = "forward_mcp_call")]
    [Description("Forward an MCP method call to a specific registered server")]
    private static async Task<IResult> ForwardMcpCall(
        [FromBody] ForwardMcpRequest request,
        IMcpForwardingService forwardingService,
        IValidator<ForwardMcpRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var response = await forwardingService.ForwardCallAsync(request);
        return Results.Ok(response);
    }

    /// <summary>
    /// Discover available tools across registered servers
    /// </summary>
    [McpServerTool(Name = "discover_mcp_tools")]
    [Description("Discover and list all available MCP tools across registered servers")]
    private static async Task<IResult> DiscoverTools(
        [FromBody] DiscoverToolsRequest request,
        IMcpForwardingService forwardingService,
        IValidator<DiscoverToolsRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var response = await forwardingService.DiscoverToolsAsync(request);
        return Results.Ok(response);
    }
}