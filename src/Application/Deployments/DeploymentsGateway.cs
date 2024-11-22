using Domain;
using Domain.Deployments;

namespace Application.Deployments;

internal class DeploymentsGateway
{
    private readonly IGetDeployments _getDeployments;

    public DeploymentsGateway(IGetDeployments getDeployments)
    {
        _getDeployments = getDeployments;
    }


    internal async Task<IList<Deployment>> GetDeployments(string projectId)
    {
        var azureDeployments = await _getDeployments.GetDeployments(projectId);

        var result = new List<Deployment>();

        foreach (var azureDeployment in azureDeployments)
            result.Add(DeploymentFactory.Create(azureDeployment.Id, azureDeployment.Stage, azureDeployment.UserId,
                azureDeployment.Date, azureDeployment.WebLink));

        return result;
    }
}