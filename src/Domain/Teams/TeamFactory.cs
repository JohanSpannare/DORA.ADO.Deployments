namespace Domain.Teams;

public static class TeamFactory
{
    public static Team Create(string id, string name)
    {
        return new Team(id, name);
    }
}