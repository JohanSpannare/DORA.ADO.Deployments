namespace Domain.Projects;

public static class ProjectFactory
{
    public static Project Create(string azureProjectId, string defaultTeamName)
    {
        return new Project(azureProjectId, defaultTeamName);
    }
}