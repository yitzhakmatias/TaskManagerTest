using System.Net;
using System.Text.Json;

namespace TaskManagement.Api.Adapters.Http;

/// <summary>
/// Catches any exception that escapes an endpoint handler and converts it into a
/// consistent application/problem+json response instead of leaking a stack trace.
/// Expected failure paths (validation, not-found) are handled directly by the
/// endpoints via typed results and never reach this middleware.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing {Method} {Path}",
                context.Request.Method, context.Request.Path);

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var problem = new
            {
                title = "An unexpected error occurred.",
                status = context.Response.StatusCode,
                traceId = context.TraceIdentifier
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}
