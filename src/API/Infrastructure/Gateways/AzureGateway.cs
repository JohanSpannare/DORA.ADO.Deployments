using Domain;
using Domain.Deployments;
using Domain.Projects;
using Domain.Teams;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using Microsoft.VisualStudio.Services.WebApi;
using Deployment = Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Deployment;

namespace API.Infrastructure.Gateways;

public class AzureGateway : IGetDeployments,IGetProjects,IGetTeams
{
    private readonly IDateTimeFactory _dateTimeFactory;
    private readonly IVssConnection _vssConnection;

    public AzureGateway(IVssConnection connection, IDateTimeFactory dateTimeFactory)
    {
        _vssConnection = connection;
        _dateTimeFactory = dateTimeFactory;
    }

    public async Task<IList<AzureDeployment>> GetDeployments(string projectId)
    {
        var client = _vssConnection.GetClient<ReleaseHttpClient2>();
        var continuationToken = 0;
        bool parsed;
        var minStartTime = _dateTimeFactory.GetCurrentUTC().AddMonths(-12);
        var deployments = new List<Deployment>();

        do
        {
            var deploymentsFromApi = await client.GetDeploymentsAsync2(Guid.Parse(projectId), null, null, null, null,
                null,
                DeploymentStatus.Succeeded, null, null, null, 100, continuationToken, null, minStartTime);

            parsed = int.TryParse(deploymentsFromApi.ContinuationToken, out continuationToken);

            deployments.AddRange(deploymentsFromApi);
        } while (continuationToken != 0 && parsed);

        var result = new List<AzureDeployment>();

        foreach (var deployment in deployments)
        {
            var azureDeployment = new AzureDeployment();

            azureDeployment.Id = deployment.Id.ToString();
            azureDeployment.UserId = deployment.RequestedFor.UniqueName;
            azureDeployment.Stage = deployment.ReleaseEnvironmentReference.Name;
            azureDeployment.Date = deployment.CompletedOn;

            if (deployment.Release.Links.Links.ContainsKey("web"))
            {
                var webLink = (ReferenceLink)deployment.Release.Links.Links["web"];

                azureDeployment.WebLink = webLink.Href;
            }

            result.Add(azureDeployment);
        }

        return result;
    }

    public async Task<IList<ProjectDto>> GetProjects()
    {
        var client = _vssConnection.GetClient<ProjectHttpClient>();

        var azureProjects = await client.GetProjects();

        var result = new List<ProjectDto>();

        foreach (var azurProject in azureProjects)
        {
            var teamProject = await client.GetProject(azurProject.Id.ToString());

            var azureProject = new ProjectDto { Id = azurProject.Id.ToString("D"),DefaultTeam = teamProject.DefaultTeam.Name };

            result.Add(azureProject);
        }

        return result;
    }

    public async Task<IList<TeamDto>> GetTeams(string projectId)
    {
        var teamHttpClient = _vssConnection.GetClient<TeamHttpClient>();

        var teamsAsync = new List<WebApiTeam>();
        List<WebApiTeam> teamPage;

        var teamsAsyncCount = 0;

        do
        {
            teamPage = await teamHttpClient.GetTeamsAsync(projectId, top: 100, skip: teamsAsyncCount);

            teamsAsync.AddRange(teamPage);

            teamsAsyncCount = teamsAsync.Count;
        } while (teamPage.Count > 0);

        

        var result = new List<TeamDto>();

        foreach (var webApiTeam in teamsAsync)
        {
            var teamDto = new TeamDto
            {
                Id = webApiTeam.Id.ToString(),
                Name = webApiTeam.Name,
                Members = new List<UserDto>()
            };


            result.Add(teamDto);


            var teamMembersWithExtendedPropertiesAsync = new List<TeamMember>();
            List<TeamMember> memberPage;

            var membersAsyncCount = 0;

            do
            {
                memberPage = await teamHttpClient.GetTeamMembersWithExtendedPropertiesAsync(
                    webApiTeam.ProjectId.ToString(), webApiTeam.Id.ToString(), 100, membersAsyncCount);

                teamMembersWithExtendedPropertiesAsync.AddRange(memberPage);

                membersAsyncCount = teamMembersWithExtendedPropertiesAsync.Count;
            } while (memberPage.Count > 0);


            foreach (var teamMember in teamMembersWithExtendedPropertiesAsync)
            {
                var userDto = new UserDto
                {
                    Id = teamMember.Identity.UniqueName
                };

                teamDto.Members.Add(userDto);
            }
        }

        return result;
    }
}