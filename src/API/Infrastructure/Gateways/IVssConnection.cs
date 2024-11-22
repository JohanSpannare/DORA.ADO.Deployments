using Microsoft.VisualStudio.Services.WebApi;

namespace API.Infrastructure.Gateways;

public interface IVssConnection : IDisposable
{
    public T GetClient<T>() where T : VssHttpClientBase
    {
        return GetClientAsync<T>().SyncResult();
    }

    public Task<T> GetClientAsync<T>(CancellationToken cancellationToken = default) where T : VssHttpClientBase;
}