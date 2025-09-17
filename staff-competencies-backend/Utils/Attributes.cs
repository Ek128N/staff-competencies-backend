using System.ComponentModel.DataAnnotations;

namespace staff_competencies_backend.Utils;

public class SkillLevelRangeAttribute : RangeAttribute
{
    public SkillLevelRangeAttribute() : base(1, 10)
    {
        ErrorMessage = "Skill level range must be between 1 and 10";
    }
}