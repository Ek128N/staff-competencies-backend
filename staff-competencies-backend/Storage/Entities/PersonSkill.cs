using staff_competencies_backend.Utils;

namespace staff_competencies_backend.Storage.Entities;

public class PersonSkill
{
    public long PersonId { get; set; }
    public Person Person { get; set; }

    public long SkillId { get; set; }
    public Skill Skill { get; set; } 

    [SkillLevelRange]
    public byte Level { get; set; }
}
