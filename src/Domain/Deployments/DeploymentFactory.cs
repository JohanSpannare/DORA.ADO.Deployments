namespace Domain.Deployments;

public static class DeploymentFactory
{
    public static Deployment Create(string id, string stage, string userId, DateTime date,
        string webLink)
    {
        return new Deployment(id, stage, userId, date, webLink);
    }
}