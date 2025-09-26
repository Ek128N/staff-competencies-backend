using Microsoft.EntityFrameworkCore;
using staff_competencies_backend.Models;
using staff_competencies_backend.Storage;

namespace staff_competencies_backend.Repositories;

public class Repository(CompetenciesDbContext context) : IRepository
{
    public async Task<List<Person>> GetAllPersonsWithSkills()
    {
        return await context.Persons
            .Include(p => p.PersonSkills)
            .ThenInclude(p => p.Skill)
            .ToListAsync();
    }

    public async Task<Person?> GetPersonWithSkills(long id)
    {
        return await context.Persons
            .Include(p => p.PersonSkills)
            .ThenInclude(p => p.Skill)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public void AddPerson(Person person)
    {
        context.Persons.Add(person);
    }

    public async Task<List<Skill>> GetSkillsByNames(IEnumerable<string> names)
    {
        return await context.Skills
            .Where(s => names.Contains(s.Name))
            .ToListAsync();
    }

    public void AddSkill(Skill skill)
    {
        context.Skills.Add(skill);
    }

    public void DeletePerson(Person person)
    {
        context.Persons.Remove(person);
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}