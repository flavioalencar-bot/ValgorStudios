using System.Net;
using System.Text.Json;
using Valgor.Application.Common.Results;

namespace Valgor.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteProblemAsync(context, exception);
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            throw exception;
        }

        var (status, error) = Map(exception);
        context.Response.Clear();
        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/json";

        var payload = new
        {
            code = error.Code,
            message = error.Message,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }

    private static (HttpStatusCode Status, Error Error) Map(Exception exception) => exception switch
    {
        UnauthorizedAccessException => (HttpStatusCode.Unauthorized, Error.Unauthorized(exception.Message)),
        KeyNotFoundException => (HttpStatusCode.NotFound, Error.NotFound(exception.Message)),
        ArgumentException => (HttpStatusCode.BadRequest, Error.Validation(exception.Message)),
        _ => (HttpStatusCode.InternalServerError, Error.Failure("Ocorreu um erro interno."))
    };
}
