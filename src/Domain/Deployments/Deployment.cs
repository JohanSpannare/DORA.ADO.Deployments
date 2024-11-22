namespace Domain.Deployments;

public class Deployment
{
    internal Deployment(string id, string stage, string userId, DateTime date, string webLink)
    {
        Link = webLink;
        Id = id;
        Stage = stage;
        UserId = userId;
        Date = date;
    }

    public string Id { get; }
    public string Stage { get; private set; }
    public string UserId { get; }
    public string Team { get; private set; }

    public string Link { get; private set; }

    public DateTime Date { get; }

    public void SetTeam(string team)
    {
        Team = team;
    }

    public void SetStage(string stage)
    {
        Stage = stage;
    }
}