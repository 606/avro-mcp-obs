using Avro.Mcp.Orchestrator.Endpoints;
using Avro.Mcp.Orchestrator.Infrastructure;
using Avro.Mcp.Orchestrator.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using ModelContextProtocol.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add HTTP client factory
builder.Services.AddHttpClient();

// Add HTTP context accessor for tenant provider
builder.Services.AddHttpContextAccessor();

// Register custom services
builder.Services.AddScoped<ITenantProvider, RouteTenantProvider>();
builder.Services.AddSingleton<IMcpServerManager, InMemoryMcpServerManager>();
builder.Services.AddScoped<IMcpForwardingService, McpForwardingService>();

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Add authentication and authorization
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization(options =>
{
    // Uncomment the following line to require authentication for all endpoints
    // options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});

// Add MCP server with HTTP transport
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

// Map orchestrator endpoints
app.MapServerManagement();
app.MapForwarding();

// Map MCP endpoint
var serviceName = "orchestrator";
app.MapMcp(pattern: $"api/{serviceName}/mcp");

// Add a health endpoint
app.MapGet("/health", () => Results.Ok(new { 
    Status = "Healthy", 
    Timestamp = DateTime.UtcNow,
    Service = "Avro MCP Orchestrator",
    Version = "1.0.0"
}))
.WithName("Health")
.WithSummary("Health check endpoint")
.Produces(StatusCodes.Status200OK);

// Add a root endpoint with service information
app.MapGet("/", () => Results.Ok(new {
    Service = "Avro MCP Orchestrator",
    Version = "1.0.0",
    Description = "MCP server orchestrator that manages and routes requests to other MCP servers",
    Endpoints = new
    {
        Health = "/health",
        MCP = $"/api/{serviceName}/mcp",
        Swagger = "/swagger",
        ServerManagement = "/servers",
        Forwarding = "/forward"
    },
    Timestamp = DateTime.UtcNow
}))
.WithName("Root")
.WithSummary("Service information")
.Produces(StatusCodes.Status200OK);

await app.RunAsync();