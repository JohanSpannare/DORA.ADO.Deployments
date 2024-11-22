using Domain.Projects;

namespace Domain.Deployments;

public interface IGetProjects
{
    Task<IList<ProjectDto>> GetProjects();
}