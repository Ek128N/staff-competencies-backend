namespace staff_competencies_backend.Dtos;

public record PersonRequestDto(string Name, string DisplayName, HashSet<SkillDto> Skills);