namespace staff_competencies_backend.Dtos;

public record PersonResponseDto(long Id, string Name, string DisplayName, HashSet<SkillDto> Skills);