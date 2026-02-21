using GoogleServices;
using LocallyGDriveApi.Shared.Helpers;

namespace LocallyGDriveApi.Shared;

public static class DependencyInjection
{
     public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration )
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IApplicationContext, ApplicationContext>();


        //Repositories
        

        //Services
        services.AddGoogleServices();
        
        return services;
    }

}
