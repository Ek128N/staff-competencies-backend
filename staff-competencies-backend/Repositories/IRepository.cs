using staff_competencies_backend.Models;

namespace staff_competencies_backend.Repositories;

public interface IRepository
{
    Task<List<Person>> GetAllPersonsWithSkills();
    Task<Person?> GetPersonWithSkills(long id );
    void AddPerson(Person person);
    void  AddSkill(Skill skill);
    Task<List<Skill>> GetSkillsByNames(IEnumerable<string> names);
    void DeletePerson(Person person);
    Task SaveChangesAsync();
}