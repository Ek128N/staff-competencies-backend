using Microsoft.EntityFrameworkCore;
using staff_competencies_backend.Storage;
using staff_competencies_backend.Storage.Entities;
using staff_competencies_backend.Utils;

namespace staff_competencies_backend.Services;

public interface IPersonService
{
    Task<List<PersonResponseDto>> GetPersons();
    Task<PersonResponseDto?> GetPerson(long id);
    Task<CreatePersonResponse> CreatePerson(PersonRequestDto person);
    Task UpdatePerson(long id, PersonRequestDto dto);
    Task DeletePerson(long id);
}

public class PersonService(CompetenciesDbContext context) : IPersonService
{
    public async Task<List<PersonResponseDto>> GetPersons()
    {
        var persons = await context.Persons.Include(p => p.PersonSkills)
            .ThenInclude(p => p.Skill)
            .ToListAsync();

        return persons.ToPersonResponseDto();
    }

    public async Task<PersonResponseDto?> GetPerson(long id)
    {
        var person = await context.Persons
            .Include(p => p.PersonSkills)
            .ThenInclude(p => p.Skill).FirstOrDefaultAsync(p => p.Id == id);
        if (person == null)
            throw new NotFoundException($"Person with id {id} not found.");

        return person.ToPersonResponseDto();
    }

    public async Task<CreatePersonResponse> CreatePerson(PersonRequestDto dto)
    {
        var person = new Person
        {
            Name = dto.Name,
            DisplayName = dto.DisplayName
        };

        await AddSkillsToPerson(person, dto.Skills);

        context.Persons.Add(person);
        await context.SaveChangesAsync();

        return new CreatePersonResponse(PersonId:person.Id);
    }

    public async Task UpdatePerson(long id, PersonRequestDto dto)
    {
        var person = await context.Persons
            .Include(p => p.PersonSkills)
            .ThenInclude(p => p.Skill).FirstOrDefaultAsync(p => p.Id == id);

        if (person == null)
            throw new NotFoundException($"Person with id {id} not found.");

        person.Name = dto.Name;
        person.DisplayName = dto.DisplayName;
        person.PersonSkills.Clear();

        await AddSkillsToPerson(person, dto.Skills);

        await context.SaveChangesAsync();
    }

    private async Task AddSkillsToPerson(Person person,HashSet<SkillDto> skillsDto)
    {
        var repeatedSkillNames = skillsDto
            .GroupBy(s => s.Name) // case-insensitive optional
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (repeatedSkillNames.Count > 0)
        {
            throw new BadRequestException(
                $"Person can't have duplicate skills: {string.Join(", ", repeatedSkillNames)}");
        }
        
        var skillsNames = skillsDto.Select(s => s.Name).ToHashSet();

        var existingSkills =
            await context.Skills.Where(s => skillsNames.Contains(s.Name)).ToDictionaryAsync(s => s.Name);

        foreach (var skillDto in skillsDto)
        {
            if (!existingSkills.TryGetValue(skillDto.Name, out var skill))
            {
                skill = new Skill { Name = skillDto.Name };
                context.Skills.Add(skill);

                existingSkills[skillDto.Name] = skill;
            }

            person.PersonSkills.Add(new PersonSkill
            {
                Skill = skill,
                Person = person,
                Level = skillDto.Level,
            });
        }
    }
    
    public async Task DeletePerson(long id)
    {
        var person = await context.Persons.FirstOrDefaultAsync(p => p.Id == id);
        if (person == null)
        {
            throw new NotFoundException($"Person with id {id} not found.");
        }

        context.Persons.Remove(person);
        await context.SaveChangesAsync();
    }
}