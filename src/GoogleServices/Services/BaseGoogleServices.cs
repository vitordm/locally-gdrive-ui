using Google.Apis.Auth.OAuth2;
using GoogleServices.Configurations;
using Microsoft.Extensions.Options;

namespace GoogleServices.Services;

public abstract class BaseGoogleServices(IOptions<GoogleServicesSettings> settings)
{
    private readonly GoogleServicesSettings _settings = settings.Value;

    protected async Task<GoogleCredential> GetGoogleCredentialAsync(CancellationToken cancellationToken)
    {
        return await CredentialFactory
                        .FromFileAsync(
                            credentialPath: _settings.JsonConfigurationPath, 
                            credentialType: JsonCredentialParameters.ServiceAccountCredentialType,
                            cancellationToken: cancellationToken
                        );
    }

    protected async Task<GoogleCredential> CreateCredentialByScopedAsync(string scoped, CancellationToken cancellationToken)
    {
        var baseCredential = await GetGoogleCredentialAsync(cancellationToken);
        return baseCredential.CreateScoped(scoped);
    }
}
