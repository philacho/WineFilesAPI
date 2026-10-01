using System.Data.OleDb;
using System.Text.Json;

namespace WineFilesApi.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    // Don't serialize these properties into the response even in dev
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException)
            when (context.RequestAborted.IsCancellationRequested)
        {
            // Client disconnected — not an error. 499 is nginx's convention.
            _logger.LogInformation(
                "Request {Method} {Path} was cancelled by the client.",
                context.Request.Method, context.Request.Path);

            if (!context.Response.HasStarted)
                context.Response.StatusCode = 499;
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        // Pick a status code based on the exception type.
        var (statusCode, title) = MapException(ex);

        // Log at the appropriate level.
        if (statusCode >= 500)
            _logger.LogError(ex, "Unhandled API exception: {Title}", title);
        else
            _logger.LogWarning(ex, "Handled API exception: {Title}", title);

        // Response may already have started (e.g. streaming) — can't write a body then.
        if (context.Response.HasStarted)
        {
            _logger.LogWarning(
                "Response already started; cannot write error body. TraceId={TraceId}",
                context.TraceIdentifier);
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        var payload = BuildPayload(context, ex, statusCode, title);

        var json = JsonSerializer.Serialize(payload, JsonOptions);
        await context.Response.WriteAsync(json, context.RequestAborted);
    }

    private object BuildPayload(
        HttpContext context, Exception ex, int statusCode, string title)
    {
        var traceId = context.TraceIdentifier;
        var isDev = _env.IsDevelopment();

        if (!isDev)
        {
            // Production: minimal, no internals leaked.
            return new
            {
                statusCode,
                message = title,
                traceId
            };
        }

        // Development: rich detail to make debugging painless.
        return new
        {
            statusCode,
            message = title,
            exception = new
            {
                type = ex.GetType().FullName,
                message = ex.Message,
                stackTrace = ex.StackTrace,
                source = ex.Source,
                innerExceptions = FlattenInner(ex)
            },
            // OLEDB-specific diagnostics — very useful for VFP issues.
            oleDb = ExtractOleDbInfo(ex),
            request = new
            {
                method = context.Request.Method,
                path = context.Request.Path.Value,
                queryString = context.Request.QueryString.Value
            },
            traceId,
            timestampUtc = DateTime.UtcNow
        };
    }

    private static IEnumerable<object> FlattenInner(Exception ex)
    {
        var inner = ex.InnerException;
        while (inner is not null)
        {
            yield return new
            {
                type = inner.GetType().FullName,
                message = inner.Message,
                stackTrace = inner.StackTrace
            };
            inner = inner.InnerException;
        }
    }

    private static object? ExtractOleDbInfo(Exception ex)
    {
        var ole = FindOleDbException(ex);
        if (ole is null) return null;

        return new
        {
            message = ole.Message,
            // OleDbException exposes per-error details (SQLSTATE / native error).
            errors = ole.Errors.Cast<OleDbError>().Select(e => new
            {
                message = e.Message,
                sqlState = e.SQLState,
                nativeError = e.NativeError,
                source = e.Source
            }).ToArray()
        };
    }

    private static OleDbException? FindOleDbException(Exception? ex)
    {
        while (ex is not null)
        {
            if (ex is OleDbException ole) return ole;
            ex = ex.InnerException;
        }
        return null;
    }

    private static (int StatusCode, string Title) MapException(Exception ex) => ex switch
    {
        // VFP / OLEDB — likely a configuration or environment problem.
        OleDbException => (StatusCodes.Status500InternalServerError,
                           "A database error occurred."),

        // Bad input from the client.
        ArgumentNullException => (StatusCodes.Status400BadRequest, "A required argument was null."),
        ArgumentException => (StatusCodes.Status400BadRequest, "Invalid argument."),
        FormatException => (StatusCodes.Status400BadRequest, "Invalid format."),
        InvalidOperationException => (StatusCodes.Status400BadRequest, "Invalid operation."),

        // Missing resources.
        KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found."),

        // AuthN / AuthZ.
        UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Access denied."),

        // Timeouts & cancellations.
        TimeoutException => (StatusCodes.Status504GatewayTimeout, "The request timed out."),

        // Fallback.
        _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
    };
}