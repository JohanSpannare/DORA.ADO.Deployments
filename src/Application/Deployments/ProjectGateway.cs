using Domain.Deployments;
using Domain.Projects;

namespace Application.Deployments;

internal class ProjectGateway
{
    private readonly IGetProjects _getProjects;

    public ProjectGateway(IGetProjects getProjects)
    {
        _getProjects = getProjects;
    }


    internal async Task<IList<Project>> GetProject()
    {
        var azureProjects = await _getProjects.GetProjects();

        var result = new List<Project>();

        foreach (var azureProject in azureProjects) result.Add(ProjectFactory.Create(azureProject.Id,azureProject.DefaultTeam));

        return result;
    }
}