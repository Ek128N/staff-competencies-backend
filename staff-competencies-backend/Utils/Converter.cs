using staff_competencies_backend.Storage.Entities;

namespace staff_competencies_backend.Utils;

public static class Converter
{
    public static List<PersonResponseDto> ToPersonResponseDto(this List<Person> persons)
    {
        return persons.Select(p => p.ToPersonResponseDto()).ToList();
    }

    public static PersonResponseDto ToPersonResponseDto(this Person person)
    {
        return new PersonResponseDto(
            Id: person.Id,
            Name: person.Name,
            DisplayName: person.DisplayName,
            Skills: person.PersonSkills
                .Select(ps => new SkillDto(
                    Name: ps.Skill.Name,
                    Level: ps.Level))
                .ToHashSet()
        );
    }
}