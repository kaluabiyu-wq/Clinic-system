using Clinic.Domain.Common;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Api.Middlleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _log;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> log)
    {
        _next = next;
        _log = log;
    }

    public async Task Invoke(HttpContext ctx)
    {
        try { await _next(ctx); }
        catch (ValidationException ex)
        {
            var errors = ex.Errors.GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            await Write(ctx, new ValidationProblemDetails(errors) { Status = 400, Title = "Validation failed" });
        }
        catch (NotFoundException ex) { await Write(ctx, Problem(404, ex.Message)); }
        catch (ConflictException ex) { await Write(ctx, Problem(409, ex.Message)); }
        catch (DomainException ex) { await Write(ctx, Problem(400, ex.Message)); }
        catch (Exception ex)
        {
            _log.LogError(ex, "Unhandled exception");
            await Write(ctx, Problem(500, "Something went wrong."));
        }
    }

    private static ProblemDetails Problem(int status, string detail) =>
        new() { Status = status, Title = detail, Detail = detail };

    private static Task Write(HttpContext ctx, ProblemDetails p)
    {
        ctx.Response.StatusCode = p.Status ?? 500;
        return ctx.Response.WriteAsJsonAsync(p);
    }
}
