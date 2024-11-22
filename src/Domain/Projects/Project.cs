namespace Domain.Projects;

public class Project
{
    internal Project(string id, string defaultTeamName)
    {
        Id = id;
        DefaultTeamName = defaultTeamName;
    }

    public string Id { get; }
    public string DefaultTeamName { get; set; }
}