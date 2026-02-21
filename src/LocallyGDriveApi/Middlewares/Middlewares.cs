using System;
using LocallyGDriveApi.Shared;
using Serilog.Context;

namespace LocallyGDriveApi.Middlewares;

public static class Middlewares
{

    public static IApplicationBuilder UseMiddlewares(this IApplicationBuilder app)
    {
        return app.UserCorrelationId()
                  .UseLogRequest();
    }

    public static IApplicationBuilder UserCorrelationId(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            if (!context.Request.Headers.TryGetValue(Constants.CORRELATION_ID, out Microsoft.Extensions.Primitives.StringValues value))
            {
                value = Ulid.NewUlid().ToString();
                context.Request.Headers[Constants.CORRELATION_ID] = value;
            }

            context.Items[Constants.CORRELATION_ID] = value;
            context.Response.Headers[Constants.CORRELATION_ID] = value;

            using (LogContext.PushProperty("correlationId", value.ToString()))
            {
                await next.Invoke();
            }
        });

        return app;
    }

    public static IApplicationBuilder UseLogRequest(this IApplicationBuilder app)
    {

        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Development")
        {
            return app;
        }

        var logger = app.ApplicationServices.GetService<ILogger<Program>>();

        if (logger == null)
            return app;

        app.Use(async (context, next) =>
        {
            var correlationId = context.Request.Headers[Constants.CORRELATION_ID].FirstOrDefault()
                                ?? context.TraceIdentifier;

            using (LogContext.PushProperty("correlationId", correlationId))
            using (LogContext.PushProperty("method", context.Request.Method))
            using (LogContext.PushProperty("path", context.Request.Path.Value))
            {
                logger.LogInformation("Incoming request");

                await next.Invoke();

                logger.LogInformation("Outgoing response with status {statusCode}", context.Response.StatusCode);
            }
        });

        return app;
    }
}
