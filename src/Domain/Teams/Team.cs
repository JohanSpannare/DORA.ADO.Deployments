namespace Domain.Teams;

public class Team

{
    private readonly IList<Member> _members;

    internal Team(string id, string name)
    {
        Id = id;
        Name = name;

        _members = new List<Member>();
    }

    public IReadOnlyList<Member> Members => _members.ToArray();

    public string Id { get; private set; }
    public string Name { get; private set; }


    public void AddMember(Member member)
    {
        _members.Add(member);
    }
}