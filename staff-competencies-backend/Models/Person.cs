namespace staff_competencies_backend.Models;

public class Person
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required string DisplayName { get; set; }

    public virtual HashSet<PersonSkill> PersonSkills { get; set; } = [];
}