using Domain.Deployments;
using Domain.Teams;

namespace Application.Deployments;

public class TeamGateway
{
    private readonly IGetTeams _getTeams;

    public TeamGateway(IGetTeams getTeams)
    {
        _getTeams = getTeams;
    }

    public async Task<IList<Team>> Get(string projectId)
    {
        var azureTeams = await _getTeams.GetTeams(projectId);


        var result = new List<Team>();
        foreach (var azureTeam in azureTeams)
        {
            var team = TeamFactory.Create(azureTeam.Id, azureTeam.Name);

            foreach (var azureTeamMember in azureTeam.Members) team.AddMember(new Member { Id = azureTeamMember.Id });

            result.Add(team);
        }

        return result;
    }
}