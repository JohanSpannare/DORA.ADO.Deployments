using Domain.Deployments;

namespace Domain;


public interface IGetDeployments
{
    Task<IList<AzureDeployment>> GetDeployments(string projectId);
}