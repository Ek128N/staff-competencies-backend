using System.Net;
using System.Net.Http.Json;
using staff_competencies_backend;

namespace IntegrationTests;

public class PersonApiTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<long> CreatePersonAsync(PersonRequestDto dto, CancellationToken ct)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/persons", dto, ct);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<CreatePersonResponse>(cancellationToken: ct);
        return created!.PersonId;
    }

    [Fact]
    public async Task GetPerson_NotExisting_Return404()
    {
        var response = await _client.GetAsync("/api/v1/persons/-1", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAllPersons_ShouldReturnList()
    {
        var person1 = new PersonRequestDto("person1 Name", "person1 DisplayName",
            [new SkillDto("person1 Skill Name", 1)]);
        var person2 = new PersonRequestDto("person2 Name", "person2 DisplayName",
            [new SkillDto("person2 Skill Name", 10)]);

        await CreatePersonAsync(person1, TestContext.Current.CancellationToken);
        await CreatePersonAsync(person2, TestContext.Current.CancellationToken);

        //Act
        var response = await _client.GetFromJsonAsync<List<PersonResponseDto>>("/api/v1/persons",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.True(response.Count >= 2);
        Assert.Contains(response, p => p.Name == person1.Name);
        Assert.Contains(response, p => p.Name == person2.Name);
    }

    [Fact]
    public async Task UpdatePerson_WithValidData_UpdatesPerson()
    {
        var newPerson = new PersonRequestDto("newPerson Name", "newPerson DisplayName",
            [new SkillDto("newPerson Skill Name", 1)]);

        var personId = await CreatePersonAsync(newPerson, TestContext.Current.CancellationToken);

        var updatedPerson = new PersonRequestDto("updatedPerson Name", "updatedPerson DisplayName",
        [
            new SkillDto("updatedPerson Skill Name 1", 10),
            new SkillDto("updatedPerson Skill Name 2", 1)
        ]);

        //Act
        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/persons/{personId}", updatedPerson,
            cancellationToken: TestContext.Current.CancellationToken);

        updateResponse.EnsureSuccessStatusCode();

        var getResponse = await _client.GetFromJsonAsync<PersonResponseDto>($"/api/v1/persons/{personId}",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(getResponse);
        Assert.Equal(updatedPerson.Name, getResponse.Name);
        Assert.Equal(updatedPerson.DisplayName, getResponse.DisplayName);
        Assert.Equal(updatedPerson.Skills.Count, getResponse.Skills.Count);

        foreach (var skill in updatedPerson.Skills)
            Assert.Contains(getResponse.Skills, s => s.Name == skill.Name && s.Level == skill.Level);
        Assert.DoesNotContain(getResponse.Skills, s => s.Name == newPerson.Skills.First().Name);
    }

    [Fact]
    public async Task DeletePerson_WithValidData_DeletesPerson()
    {
        var newPerson = new PersonRequestDto("newPerson Name", "newPerson DisplayName",
            [new SkillDto("newPerson Skill Name 1", 10), new SkillDto("newPerson Skill Name 2", 1)]);

        var personId = await CreatePersonAsync(newPerson, TestContext.Current.CancellationToken);

        var getPersonResponseExists =
            await _client.GetAsync($"/api/v1/persons/{personId}", TestContext.Current.CancellationToken);
        getPersonResponseExists.EnsureSuccessStatusCode();

        //Act 
        var deleteResponse =
            await _client.DeleteAsync($"/api/v1/persons/{personId}", TestContext.Current.CancellationToken);
        deleteResponse.EnsureSuccessStatusCode();

        var getPersonResponseDeleted =
            await _client.GetAsync($"/api/v1/persons/{personId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, getPersonResponseDeleted.StatusCode);
    }

    [Fact]
    public async Task CreatePerson_WithInvalidSkillLevel_Return400()
    {
        var invalidPerson = new
        {
            Name = "Person Name",
            DisplayName = "Person DisplayName",
            Skills = new[] { new { Name = "Skill Name", Level = 15 } }
        };

        //Act
        var response = await _client.PostAsJsonAsync("/api/v1/persons", invalidPerson,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePerson_WithDuplicateSkills_Return400()
    {
        var personWithDuplicateSkills = new PersonRequestDto("Person Name", "Person DisplayName",
        [
            new SkillDto("Duplicated Skill Name", 1),
            new SkillDto("Duplicated Skill Name", 2)
        ]);

        //Act
        var createResponse = await _client.PostAsJsonAsync("/api/v1/persons", personWithDuplicateSkills,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, createResponse.StatusCode);
    }
}