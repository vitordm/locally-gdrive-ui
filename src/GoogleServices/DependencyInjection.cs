using GoogleServices.Configurations;
using GoogleServices.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GoogleServices;

public static class DependencyInjection
{
    public static IServiceCollection AddGoogleServices(this IServiceCollection servicesCollection)
    {
        servicesCollection
            .AddOptions<GoogleServicesSettings>()
            .Configure(options =>
            {
                options.JsonConfigurationPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS") ?? string.Empty;
            })
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.JsonConfigurationPath),
                "Environment variable 'GOOGLE_APPLICATION_CREDENTIALS' must be configured."
            )
            .ValidateOnStart();

        servicesCollection.AddScoped<IGoogleDriveService, GoogleDriveService>();
        return servicesCollection;
    }
}
