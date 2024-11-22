using Domain.Teams;

namespace Domain.Deployments;

public interface IGetTeams
{
    Task<IList<TeamDto>> GetTeams(string projectId);
}