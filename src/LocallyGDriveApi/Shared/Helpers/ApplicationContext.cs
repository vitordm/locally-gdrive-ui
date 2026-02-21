using System;

namespace LocallyGDriveApi.Shared.Helpers;

public interface IApplicationContext
{
    string? CorrelationID();
}

public class ApplicationContext(IHttpContextAccessor httpContextAccessor) : IApplicationContext
{
    private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;

    public string? CorrelationID()
     => httpContextAccessor.HttpContext?.Items[Constants.CORRELATION_ID] as string;
}
