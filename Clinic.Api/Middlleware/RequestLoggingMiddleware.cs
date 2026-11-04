using System.Diagnostics;
using System.Security.Claims;

namespace Clinic.Api.Middlleware;


public class RequestLoggingMiddleware
{
   
    private static readonly string[] SkippedPrefixes = { "/health", "/swagger", "/favicon" };

    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldSkip(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var startTimestamp = Stopwatch.GetTimestamp();
        var threw = false;

        try
        {
            await _next(context);
        }
        catch
        {
            
            threw = true;
            throw;
        }
        finally
        {
            var elapsedMs = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;
            var statusCode = threw ? StatusCodes.Status500InternalServerError : context.Response.StatusCode;

            var route = GetRouteTemplate(context);
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";

            var level = statusCode >= 500 ? LogLevel.Error
                      : statusCode >= 400 ? LogLevel.Warning
                      : LogLevel.Information;

            _logger.Log(
                level,
                "HTTP {Method} {Route} responded {StatusCode} in {ElapsedMs:0.0} ms (user {UserId}, trace {TraceId})",
                context.Request.Method,
                route,
                statusCode,
                elapsedMs,
                userId,
                context.TraceIdentifier);
        }
    }

    private static bool ShouldSkip(PathString path)
    {
        foreach (var prefix in SkippedPrefixes)
        {
            if (path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static string GetRouteTemplate(HttpContext context)
    {
        if (context.GetEndpoint() is RouteEndpoint endpoint && endpoint.RoutePattern.RawText is { } template)
            return template;

        return "(unmatched)";
    }
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app) =>
        app.UseMiddleware<RequestLoggingMiddleware>();
}