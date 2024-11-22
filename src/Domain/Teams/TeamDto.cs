namespace Domain.Teams;

public class TeamDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<UserDto> Members { get; set; }
}