using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Logging.Shared;

public class OpenTelemetryTraceIdMiddleware
{
    private readonly RequestDelegate _next;

    public OpenTelemetryTraceIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<OpenTelemetryTraceIdMiddleware>>();

        using (logger.BeginScope("{@traceId}", Activity.Current?.TraceId.ToString()))
        {
            await _next(context);
        }
    }
}