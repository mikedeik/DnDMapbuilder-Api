using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DnDMapBuilder.Infrastructure.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses.
/// </summary>
public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
    private readonly HashSet<string> _excludedPaths;
    private const int MaxBodySizeToLog = 4096; // 4KB

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _excludedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "/health",
            "/health/live",
            "/health/ready"
        };
    }

    /// <summary>
    /// Invokes the middleware to log request/response information.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        // Skip logging for excluded paths
        if (_excludedPaths.Any(path => context.Request.Path.StartsWithSegments(path)))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var correlationId = GetOrCreateCorrelationId(context);

        // Log request
        await LogRequestAsync(context, correlationId);

        // Store original response body stream
        var originalBodyStream = context.Response.Body;
        using (var responseBody = new MemoryStream())
        {
            context.Response.Body = responseBody;

            try
            {
                // Call next middleware
                await _next(context);

                // Log response
                stopwatch.Stop();
                await LogResponseAsync(context, correlationId, stopwatch.ElapsedMilliseconds);

                // Copy response to original stream
                await responseBody.CopyToAsync(originalBodyStream);
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }
    }

    /// <summary>
    /// Logs the HTTP request details.
    /// </summary>
    private async Task LogRequestAsync(HttpContext context, string correlationId)
    {
        var request = context.Request;
        var userIdentity = context.User?.Identity?.Name ?? "Anonymous";

        // Log basic request info
        _logger.LogInformation(
            "HTTP Request | CorrelationId: {CorrelationId} | Method: {Method} | Path: {Path} | Query: {Query} | User: {User}",
            correlationId,
            request.Method,
            request.Path,
            request.QueryString.Value,
            userIdentity
        );

        // Log request headers (excluding sensitive ones)
        var sensitiveHeaders = new[] { "authorization", "cookie", "x-api-key", "x-token" };
        var headers = request.Headers
            .Where(h => !sensitiveHeaders.Contains(h.Key.ToLower()))
            .ToDictionary(h => h.Key, h => h.Value.ToString());

        if (headers.Any())
        {
            _logger.LogDebug(
                "HTTP Request Headers | CorrelationId: {CorrelationId} | Headers: {@Headers}",
                correlationId,
                headers
            );
        }

        // Log request body for POST/PUT/PATCH
        if (context.Request.ContentLength > 0 &&
            (request.Method == HttpMethods.Post ||
             request.Method == HttpMethods.Put ||
             request.Method == HttpMethods.Patch))
        {
            request.EnableBuffering();
            var body = await new StreamReader(request.Body).ReadToEndAsync();
            request.Body.Position = 0;

            if (!string.IsNullOrWhiteSpace(body) && body.Length <= MaxBodySizeToLog)
            {
                _logger.LogDebug(
                    "HTTP Request Body | CorrelationId: {CorrelationId} | Body: {Body}",
                    correlationId,
                    body
                );
            }
            else if (body.Length > MaxBodySizeToLog)
            {
                _logger.LogDebug(
                    "HTTP Request Body | CorrelationId: {CorrelationId} | Body: [Truncated - Size: {Size} bytes]",
                    correlationId,
                    body.Length
                );
            }
        }
    }

    /// <summary>
    /// Logs the HTTP response details.
    /// </summary>
    private async Task LogResponseAsync(HttpContext context, string correlationId, long elapsedMilliseconds)
    {
        var response = context.Response;

        // Log response status
        _logger.LogInformation(
            "HTTP Response | CorrelationId: {CorrelationId} | StatusCode: {StatusCode} | Duration: {DurationMs}ms",
            correlationId,
            response.StatusCode,
            elapsedMilliseconds
        );

        // Log response headers
        var responseHeaders = response.Headers
            .ToDictionary(h => h.Key, h => h.Value.ToString());

        if (responseHeaders.Any())
        {
            _logger.LogDebug(
                "HTTP Response Headers | CorrelationId: {CorrelationId} | Headers: {@Headers}",
                correlationId,
                responseHeaders
            );
        }

        // Log response body for error responses or small successful responses
        if (response.Body.CanSeek)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            if (!string.IsNullOrWhiteSpace(responseBody) && responseBody.Length <= MaxBodySizeToLog)
            {
                if (response.StatusCode >= 400 || response.StatusCode == 201)
                {
                    _logger.LogDebug(
                        "HTTP Response Body | CorrelationId: {CorrelationId} | Body: {Body}",
                        correlationId,
                        responseBody
                    );
                }
            }
        }
    }

    /// <summary>
    /// Gets or creates a correlation ID for request tracing.
    /// </summary>
    private string GetOrCreateCorrelationId(HttpContext context)
    {
        const string correlationIdHeader = "X-Correlation-ID";

        if (context.Request.Headers.TryGetValue(correlationIdHeader, out var correlationId))
        {
            return correlationId.ToString();
        }

        var newCorrelationId = Guid.NewGuid().ToString();
        context.Request.Headers.Add(correlationIdHeader, newCorrelationId);
        context.Response.Headers.Add(correlationIdHeader, newCorrelationId);

        return newCorrelationId;
    }
}

/// <summary>
/// Extension methods for adding request/response logging middleware.
/// </summary>
public static class RequestResponseLoggingMiddlewareExtensions
{
    /// <summary>
    /// Adds request/response logging middleware to the pipeline.
    /// </summary>
    public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
    }
}
