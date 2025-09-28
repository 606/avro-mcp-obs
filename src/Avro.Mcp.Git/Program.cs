using Avro.Mcp.Git.Endpoints;
using Avro.Mcp.Git.Infrastructure;
using Avro.Mcp.Git.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;

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
builder.Services.AddScoped<IGitService, GitService>();

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

// MCP server setup (simplified for now)
// TODO: Add MCP server when package API is stable

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

// Map Git endpoints
app.MapGitRepository();
app.MapGitCommits();
app.MapGitBranches();
app.MapGitRemote();

// TODO: Map MCP endpoint when package API is stable
var serviceName = "git";

// Add a health endpoint
app.MapGet("/health", () => Results.Ok(new { 
    Status = "Healthy", 
    Timestamp = DateTime.UtcNow,
    Service = "Avro Git MCP Server",
    Version = "1.0.0"
}))
.WithName("Health")
.WithSummary("Health check endpoint")
.Produces(StatusCodes.Status200OK);

// Add a root endpoint with service information
app.MapGet("/", () => Results.Ok(new {
    Service = "Avro Git MCP Server",
    Version = "1.0.0",
    Description = "MCP server for Git operations including repository management, commits, branches, and remote operations",
    Endpoints = new
    {
        Health = "/health",
        Swagger = "/swagger",
        Repository = "/git/repository",
        Commits = "/git/commits",
        Branches = "/git/branches",
        Remote = "/git/remote"
    },
    Timestamp = DateTime.UtcNow
}))
.WithName("Root")
.WithSummary("Service information")
.Produces(StatusCodes.Status200OK);

await app.RunAsync();