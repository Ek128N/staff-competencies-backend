using staff_competencies_backend.Dtos;
using staff_competencies_backend.Models;
using staff_competencies_backend.Repositories;
using staff_competencies_backend.Utils;

namespace staff_competencies_backend.Services;

public class PersonService(IRepository repository) : IPersonService
{
    public async Task<List<PersonResponseDto>> GetPersons()
    {
        var persons = await repository.GetAllPersonsWithSkills();
        return persons.ToPersonResponseDto();
    }

    public async Task<PersonResponseDto> GetPerson(long id)
    {
        var person = await GetPersonWithSkills(id);
        return person.ToPersonResponseDto();
    }

    private async Task<Person> GetPersonWithSkills(long id)
    {
        var person = await repository.GetPersonWithSkills(id);
        return person ?? throw new NotFoundException($"Person with id {id} not found.");
    }

    public async Task<CreatePersonResponse> CreatePerson(PersonRequestDto dto)
    {
        var person = new Person
        {
            Name = dto.Name,
            DisplayName = dto.DisplayName
        };

        await AddSkillsToPerson(person, dto.Skills);

        repository.AddPerson(person);
        await repository.SaveChangesAsync();

        return new CreatePersonResponse(PersonId: person.Id);
    }

    private async Task AddSkillsToPerson(Person person, HashSet<SkillDto> skillsDto)
    {
        ValidateSkillNames(skillsDto);

        var skillsNames = skillsDto.Select(s => s.Name).ToHashSet();

        var skills = await repository.GetSkillsByNames(skillsNames);
        var existingSkills = skills
            .ToDictionary(s => s.Name, s => s);

        foreach (var skillDto in skillsDto)
            EnsureSkillExists(skillDto, person, existingSkills);
    }

    public async Task UpdatePerson(long id, PersonRequestDto dto)
    {
        var person = await GetPersonWithSkills(id);

        person.Name = dto.Name;
        person.DisplayName = dto.DisplayName;

        await UpdatePersonSkills(person, dto.Skills);
        await repository.SaveChangesAsync();
    }

    private async Task UpdatePersonSkills(Person person, HashSet<SkillDto> skillsDto)
    {
        ValidateSkillNames(skillsDto);

        var currentSkills = person.PersonSkills
            .ToDictionary(ps => ps.Skill.Name, ps => ps);

        var updateSkills = skillsDto
            .ToDictionary(s => s.Name, s => s);

        var removedSkills = currentSkills.Keys.Except(updateSkills.Keys);
        foreach (var skillName in removedSkills)
            person.PersonSkills.Remove(currentSkills[skillName]);


        var skills = await repository.GetSkillsByNames(updateSkills.Keys);
        var existingSkills = skills
            .ToDictionary(s => s.Name, s => s);

        foreach (var skillDto in skillsDto)
        {
            if (currentSkills.TryGetValue(skillDto.Name, out var existingSkill))
            {
                if (existingSkill.Level != skillDto.Level)
                    existingSkill.Level = skillDto.Level;
            }
            else
            {
                EnsureSkillExists(skillDto, person, existingSkills);
            }
        }
    }

    private void ValidateSkillNames(HashSet<SkillDto> skillsDto)
    {
        var repeatedSkillNames = skillsDto
            .GroupBy(s => s.Name)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (repeatedSkillNames.Count > 0)
        {
            throw new BadRequestException(
                $"Person can't have duplicate skills: {string.Join(", ", repeatedSkillNames)}");
        }
    }

    private void EnsureSkillExists(SkillDto skillDto, Person person, Dictionary<string, Skill> existingSkills)
    {
        if (!existingSkills.TryGetValue(skillDto.Name, out var skill))
        {
            skill = new Skill { Name = skillDto.Name };
            repository.AddSkill(skill);
            existingSkills[skillDto.Name] = skill;
        }

        var personSkill = new PersonSkill
        {
            Skill = skill,
            Person = person,
            Level = skillDto.Level
        };

        person.PersonSkills.Add(personSkill);
    }

    public async Task DeletePerson(long id)
    {
        var person = await GetPersonWithSkills(id);

        repository.DeletePerson(person);
        await repository.SaveChangesAsync();
    }
}