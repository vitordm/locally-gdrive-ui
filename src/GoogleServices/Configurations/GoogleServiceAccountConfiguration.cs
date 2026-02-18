using System;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;

namespace GoogleServices.Configurations;

public sealed class GoogleServiceAccountConfiguration : JsonCredentialParameters
{
    /*
    [JsonProperty("type")]
    public required string Type { get; set; }

    [JsonProperty("project_id")]
    public required string ProjectId { get; set; }

    [JsonProperty("private_key_id")]
    public required string PrivateKeyId { get; set; }

    [JsonProperty("private_key")]
    public required string PrivateKey { get; set; }

    [JsonProperty("client_email")]
    public required string ClientEmail { get; set; }

    [JsonProperty("client_id")]
    public required string ClientId { get; set; }

    [JsonProperty("auth_uri")]
    public required Uri AuthUri { get; set; }

    [JsonProperty("token_uri")]
    public required Uri TokenUri { get; set; }

    [JsonProperty("auth_provider_x509_cert_url")]
    public required Uri AuthProviderX509CertUrl { get; set; }

    [JsonProperty("client_x509_cert_url")]
    public required Uri ClientX509CertUrl { get; set; }

    [JsonProperty("universe_domain")]
    public required string UniverseDomain { get; set; }
    */
}
