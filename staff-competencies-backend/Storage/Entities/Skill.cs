namespace staff_competencies_backend.Storage.Entities;

public class Skill
{
    public long Id { get; set; }
    public required string Name { get; set; }

    public virtual HashSet<PersonSkill> PersonSkills { get; set; } = [];

}
