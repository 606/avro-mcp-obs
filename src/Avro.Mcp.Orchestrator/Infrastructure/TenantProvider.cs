namespace Avro.Mcp.Orchestrator.Infrastructure;

/// <summary>
/// Interface for providing tenant context
/// </summary>
public interface ITenantProvider
{
    /// <summary>
    /// Gets the current tenant ID
    /// </summary>
    /// <returns>The tenant ID or empty string if not available</returns>
    string GetTenantId();
}

/// <summary>
/// Tenant provider that extracts tenant ID from route/query parameters
/// </summary>
public class RouteTenantProvider(IHttpContextAccessor httpContextAccessor) : ITenantProvider
{
    public string GetTenantId()
    {
        return httpContextAccessor.HttpContext is null
            ? string.Empty
            : GetTenantId(httpContextAccessor.HttpContext);
    }

    private static string GetTenantId(HttpContext httpContext)
    {
        // First try to get from query parameter
        if (httpContext.Request.Query.TryGetValue("tenantId", out var queryTenantId) && !string.IsNullOrEmpty(queryTenantId))
        {
            return queryTenantId.ToString();
        }

        // Then try to get from route values
        if (httpContext.Request.RouteValues.TryGetValue("tenantId", out var routeTenantId) && routeTenantId is not null)
        {
            return routeTenantId.ToString() ?? string.Empty;
        }

        // Finally try to get from headers
        if (httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var headerTenantId) && !string.IsNullOrEmpty(headerTenantId))
        {
            return headerTenantId.ToString();
        }

        return string.Empty;
    }
}