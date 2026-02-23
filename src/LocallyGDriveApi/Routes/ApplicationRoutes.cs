using System;
using ChatService.Routes;

namespace LocallyGDriveApi.Routes;

public static class ApplicationRoutes
{
    public static void RegisterApplicatonRoutes(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", () => "Welcome to LGD.API!");

        endpoints.MapGet("/health", () => Results.Ok("OK"))
            .WithName("HealthCheck");

        endpoints.MapMethods("/ping", ["GET", "POST"], () => Results.Ok("Pong"))
            .WithName("Ping");

        endpoints.RegisterChatHubs();
    }

   

}
