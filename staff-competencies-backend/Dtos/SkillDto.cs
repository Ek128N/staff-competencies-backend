using staff_competencies_backend.Utils;

namespace staff_competencies_backend.Dtos;

public record SkillDto(string Name, [SkillLevelRange] byte Level);