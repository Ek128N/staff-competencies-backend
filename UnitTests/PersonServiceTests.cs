using Microsoft.EntityFrameworkCore;
using staff_competencies_backend.Dtos;
using staff_competencies_backend.Models;
using staff_competencies_backend.Services;
using staff_competencies_backend.Storage;
using staff_competencies_backend.Utils;

namespace UnitTests;

public class PersonServiceTests : IDisposable
{
    private readonly PersonService _personService;
    private readonly CompetenciesDbContext _context;

    public PersonServiceTests()
    {
        var options = new DbContextOptionsBuilder<CompetenciesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CompetenciesDbContext(options);
        _personService = new PersonService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetPersons_NoPersonsExist_ReturnsEmptyList()
    {
        var result = await _personService.GetPersons();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPersons_PersonsExist_ReturnsAllPersons()
    {
        var skill = new Skill { Id = 1, Name = "Skill Name" };
        var person = new Person
        {
            Id = 1,
            Name = "Person Name",
            DisplayName = "Person DisplayName",
            PersonSkills = [new PersonSkill { PersonId = 1, SkillId = 1, Skill = skill, Level = 1 }]
        };

        _context.Skills.Add(skill);
        _context.Persons.Add(person);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Act
        var result = await _personService.GetPersons();

        Assert.NotNull(result);
        Assert.Single(result);

        var personRes = result.First();
        Assert.Equal(person.Name, personRes.Name);
        Assert.Equal(person.DisplayName, personRes.DisplayName);
        Assert.Single(personRes.Skills);

        var skillRes = personRes.Skills.First();
        Assert.Equal(skill.Name, skillRes.Name);
        Assert.Equal(person.PersonSkills.First().Level, skillRes.Level);
    }

    [Fact]
    public async Task GetPerson_PersonExists_ReturnsPersonWithSkills()
    {
        var skill = new Skill { Id = 1, Name = "Skill Name" };
        var person = new Person
        {
            Id = 1,
            Name = "Person Name",
            DisplayName = "Person Display Name",
            PersonSkills = [new PersonSkill { PersonId = 1, SkillId = 1, Skill = skill, Level = 1 }]
        };

        _context.Skills.Add(skill);
        _context.Persons.Add(person);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Act
        var result = await _personService.GetPerson(1);

        Assert.NotNull(result);
        Assert.Equal(person.Name, result.Name);
        Assert.Equal(person.DisplayName, result.DisplayName);
        Assert.Single(result.Skills);
        Assert.Equal(skill.Name, result.Skills.First().Name);
        Assert.Equal(person.PersonSkills.First().Level, result.Skills.First().Level);
    }

    [Fact]
    public async Task GetPerson_PersonDoesNotExist_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _personService.GetPerson(-1));
    }


    [Fact]
    public async Task CreatePerson_WithValidData_CreatesPersonSuccessfully()
    {
        var personDto = new PersonRequestDto(Name: "Person Name",
            DisplayName: "Person Display Name",
            Skills:
            [
                new SkillDto(Name: "Skill Name 1", Level: 1),
                new SkillDto(Name: "Skill Name 10", Level: 10)
            ]);

        //Act
        var result = await _personService.CreatePerson(personDto);

        Assert.NotNull(result);
        Assert.True(result.PersonId > 0);

        var createdPerson = await _context.Persons
            .Include(p => p.PersonSkills)
            .ThenInclude(ps => ps.Skill)
            .FirstOrDefaultAsync(p => p.Id == result.PersonId,
                cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(createdPerson);
        Assert.Equal(personDto.Name, createdPerson.Name);
        Assert.Equal(personDto.DisplayName, createdPerson.DisplayName);
        Assert.Equal(personDto.Skills.Count, createdPerson.PersonSkills.Count);
    }

    [Fact]
    public async Task CreatePerson_WithExistingSkills_ReusesExistingSkills()
    {
        var existingSkill = new Skill { Id = 1, Name = "Skill Name 1" };
        _context.Skills.Add(existingSkill);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var personDto = new PersonRequestDto(Name: "Person Name",
            DisplayName: "Person Display Name",
            Skills:
            [
                new SkillDto(Name: existingSkill.Name, Level: 1),
                new SkillDto(Name: "Skill Name 10", Level: 10)
            ]);

        //Act
        var result = await _personService.CreatePerson(personDto);

        var skillsCount = await _context.Skills.CountAsync(cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(2, skillsCount);

        var createdPerson = await _context.Persons
            .Include(p => p.PersonSkills)
            .ThenInclude(ps => ps.Skill)
            .FirstOrDefaultAsync(p => p.Id == result.PersonId,
                cancellationToken: TestContext.Current.CancellationToken);

        var skillRes = createdPerson?.PersonSkills.First(ps => ps.Skill.Name == existingSkill.Name);
        if (skillRes != null) Assert.Equal(existingSkill.Id, skillRes.SkillId);
    }

    [Fact]
    public async Task CreatePerson_WithNoSkills_CreatesPersonWithoutSkills()
    {
        var dto = new PersonRequestDto(Name: "Person Name",
            DisplayName: "Person Display Name",
            Skills: []);

        //Act
        var result = await _personService.CreatePerson(dto);

        var createdPerson = await _context.Persons
            .Include(p => p.PersonSkills)
            .FirstOrDefaultAsync(p => p.Id == result.PersonId,
                cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(createdPerson);
        Assert.Empty(createdPerson.PersonSkills);
    }

    [Fact]
    public async Task UpdatePerson_PersonExists_UpdatesSuccessfully()
    {
        var skill1 = new Skill { Id = 1, Name = "Old Skill Name" };
        var person = new Person
        {
            Id = 1,
            Name = "Old Name",
            DisplayName = "Old DisplayName",
            PersonSkills = [new PersonSkill { PersonId = 1, SkillId = 1, Skill = skill1, Level = 1 }]
        };

        _context.Skills.Add(skill1);
        _context.Persons.Add(person);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var updateDto = new PersonRequestDto(Name: "Updated Name",
            DisplayName: "Updated DisplayName",
            Skills: [new SkillDto(Name: "Updated Skill Name", Level: 10)]);

        //Act
        await _personService.UpdatePerson(1, updateDto);

        var updatedPerson = await _context.Persons
            .Include(p => p.PersonSkills)
            .ThenInclude(ps => ps.Skill)
            .FirstOrDefaultAsync(p => p.Id == 1, cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(updatedPerson);
        Assert.Equal(updateDto.Name, updatedPerson.Name);
        Assert.Equal(updateDto.DisplayName, updatedPerson.DisplayName);
        Assert.Single(updatedPerson.PersonSkills);
        Assert.Equal(updateDto.Skills.First().Name, updatedPerson.PersonSkills.First().Skill.Name);
        Assert.Equal(updateDto.Skills.First().Level, updatedPerson.PersonSkills.First().Level);
    }

    [Fact]
    public async Task UpdatePerson_PersonDoesNotExist_ThrowsNotFoundException()
    {
        var dto = new PersonRequestDto(Name: "Updated Name",
            DisplayName: "Updated DisplayName",
            Skills: []
        );

        await Assert.ThrowsAsync<NotFoundException>(() => _personService.UpdatePerson(-1, dto));
    }

    [Fact]
    public async Task DeletePerson_PersonExists_DeletesSuccessfully()
    {
        var person = new Person
        {
            Id = 1,
            Name = "Person Name",
            DisplayName = "Person DisplayName"
        };

        _context.Persons.Add(person);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Act
        await _personService.DeletePerson(1);

        var deletedPerson = await _context.Persons.FirstOrDefaultAsync(p => p.Id == 1,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Null(deletedPerson);
    }

    [Fact]
    public async Task DeletePerson_PersonDoesNotExist_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _personService.DeletePerson(-1));
    }

    [Fact]
    public async Task DeletePerson_WithValidData_DeletesPerson()
    {
        var skill = new Skill { Id = 1, Name = "Skill Name" };
        var person = new Person
        {
            Id = 1,
            Name = "Person Name",
            DisplayName = "Person DisplayName",
            PersonSkills = [new PersonSkill { PersonId = 1, SkillId = 1, Skill = skill, Level = 1 }]
        };

        _context.Skills.Add(skill);
        _context.Persons.Add(person);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Act
        await _personService.DeletePerson(1);

        var deletedPerson = await _context.Persons.FirstOrDefaultAsync(p => p.Id == 1,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Null(deletedPerson);
    }
}