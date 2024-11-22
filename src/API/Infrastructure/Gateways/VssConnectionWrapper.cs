using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace API.Infrastructure.Gateways;

public class VssConnectionWrapper : VssConnection, IVssConnection
{
    public VssConnectionWrapper(Uri baseUrl, VssCredentials credentials) : base(baseUrl, credentials)
    {
    }
}