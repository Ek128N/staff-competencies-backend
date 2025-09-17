using staff_competencies_backend.Utils;

namespace staff_competencies_backend;

public record PersonRequestDto(string Name, string DisplayName, HashSet<SkillDto> Skills);

public record SkillDto(string Name, [SkillLevelRange] byte Level);

public record PersonResponseDto(long Id, string Name, string DisplayName, HashSet<SkillDto> Skills);
public record CreatePersonResponse(long PersonId);