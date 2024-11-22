using Domain;
using Domain.Deployments;
using Domain.Teams;
using Microsoft.Extensions.Logging;

namespace Application.Deployments;

public class DeploymentService
{
    private readonly DeploymentRepository _deploymentRepository;
    private readonly DeploymentsGateway _deploymentsGateway;
    private readonly ILogger<DeploymentService> _logger;
    private readonly ProjectGateway _projectGateway;
    private readonly TeamGateway _teamGateway;

    private readonly ValidEnvironments _validEnvironments;

    public DeploymentService(IGetDeployments getDeployments,IGetProjects getProjects,IGetTeams getTeams, IDeploymentStorage deploymentStorage,
        ILogger<DeploymentService> logger, ValidEnvironments validEnvironments)
    {
        _logger = logger;
        _deploymentRepository = new DeploymentRepository(deploymentStorage);
        _deploymentsGateway = new DeploymentsGateway(getDeployments);
        _projectGateway = new ProjectGateway(getProjects);
        _teamGateway = new TeamGateway(getTeams);

        _validEnvironments = validEnvironments;
    }

    public async Task Process()
    {
        var projects = await _projectGateway.GetProject();


        foreach (var project in projects)
        {
            _logger.LogInformation("Processing project [Id: {projectId}]", project.ToJson());
            var deployments = await _deploymentsGateway.GetDeployments(project.Id);

            var teams = await _teamGateway.Get(project.Id);

            foreach (var deployment in deployments)
            {
                var translatedEnvironment = _validEnvironments.GetEnvironment(deployment.Stage);

                if (string.IsNullOrEmpty(translatedEnvironment))
                {
                    if (translatedEnvironment == null)
                        _logger.LogWarning(
                            "Could not translate environment [Untranslated Environment: {UntranslatedEnvironment}, Link:{webLink}]",
                            deployment.Stage, deployment.Link);

                    if (translatedEnvironment == "")
                        _logger.LogDebug("Skipping ignored environment [Environment: {IgnoredEnvironment}]",
                            deployment.Stage);
                    continue;
                }

                deployment.SetStage(translatedEnvironment);

                var userTeam = new List<Team>();

                foreach (var team in teams)
                    if (team.Members.Any(x => x.Id == deployment.UserId))
                        userTeam.Add(team);

                if (userTeam.Count > 1)
                {
                    var teamsString = string.Join("|", userTeam.Select(x => x.Name));

                    _logger.LogWarning("User is member of multiple teams [User: {user}, Teams: {teams}]",
                        deployment.UserId, teamsString);
                }

                if (userTeam.Count == 0){}
                    _logger.LogWarning("User is not a member of any teams [User: {user}]", deployment.UserId);


                deployment.SetTeam((userTeam.Count == 0) | (userTeam.Count > 1)
                    ? project.DefaultTeamName
                    : userTeam.First().Name);

                _deploymentRepository.Save(deployment);
            }
        }
    }
}