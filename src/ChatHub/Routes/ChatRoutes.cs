using System;
using ChatService.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace ChatService.Routes;

public static class ChatRoutes
{
    public static void RegisterChatHubs(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHub<ChatHub>("/chat");
    }
}
