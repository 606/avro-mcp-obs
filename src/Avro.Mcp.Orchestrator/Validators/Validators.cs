using Avro.Mcp.Orchestrator.Models;
using FluentValidation;

namespace Avro.Mcp.Orchestrator.Validators;

/// <summary>
/// Validator for server registration requests
/// </summary>
public class RegisterServerRequestValidator : AbstractValidator<RegisterServerRequest>
{
    public RegisterServerRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Server name is required")
            .MaximumLength(100)
            .WithMessage("Server name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Server description is required")
            .MaximumLength(500)
            .WithMessage("Server description cannot exceed 500 characters");

        RuleFor(x => x.BaseUrl)
            .NotEmpty()
            .WithMessage("Server base URL is required")
            .Must(BeValidUrl)
            .WithMessage("Server base URL must be a valid HTTP or HTTPS URL");

        RuleFor(x => x.Version)
            .MaximumLength(50)
            .WithMessage("Version cannot exceed 50 characters");

        RuleFor(x => x.Capabilities)
            .NotNull()
            .WithMessage("Capabilities list cannot be null");

        RuleFor(x => x.Metadata)
            .NotNull()
            .WithMessage("Metadata dictionary cannot be null");
    }

    private static bool BeValidUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}

/// <summary>
/// Validator for get servers requests
/// </summary>
public class GetServersRequestValidator : AbstractValidator<GetServersRequest>
{
    public GetServersRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size cannot exceed 100");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(200)
            .WithMessage("Search term cannot exceed 200 characters");
    }
}

/// <summary>
/// Validator for MCP forwarding requests
/// </summary>
public class ForwardMcpRequestValidator : AbstractValidator<ForwardMcpRequest>
{
    public ForwardMcpRequestValidator()
    {
        RuleFor(x => x.ServerId)
            .NotEmpty()
            .WithMessage("Server ID is required");

        RuleFor(x => x.Method)
            .NotEmpty()
            .WithMessage("MCP method is required")
            .MaximumLength(100)
            .WithMessage("MCP method cannot exceed 100 characters");

        RuleFor(x => x.Parameters)
            .NotNull()
            .WithMessage("Parameters dictionary cannot be null");

        RuleFor(x => x.TenantId)
            .MaximumLength(100)
            .WithMessage("Tenant ID cannot exceed 100 characters");
    }
}

/// <summary>
/// Validator for health check requests
/// </summary>
public class HealthCheckRequestValidator : AbstractValidator<HealthCheckRequest>
{
    public HealthCheckRequestValidator()
    {
        RuleFor(x => x.ServerId)
            .NotEmpty()
            .WithMessage("Server ID is required");
    }
}

/// <summary>
/// Validator for tool discovery requests
/// </summary>
public class DiscoverToolsRequestValidator : AbstractValidator<DiscoverToolsRequest>
{
    public DiscoverToolsRequestValidator()
    {
        RuleFor(x => x.SearchTerm)
            .MaximumLength(200)
            .WithMessage("Search term cannot exceed 200 characters");

        RuleFor(x => x.Category)
            .MaximumLength(100)
            .WithMessage("Category cannot exceed 100 characters");

        RuleFor(x => x.ServerIds)
            .NotNull()
            .WithMessage("Server IDs list cannot be null");

        RuleForEach(x => x.ServerIds)
            .NotEmpty()
            .WithMessage("Server ID cannot be empty");
    }
}