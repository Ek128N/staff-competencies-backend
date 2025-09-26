using Microsoft.AspNetCore.Mvc;
using staff_competencies_backend.Dtos;
using staff_competencies_backend.Services;

namespace staff_competencies_backend.Controllers;

[Route("api/v1/persons")]
[ApiController]
public class PersonController(IPersonService personService) : ControllerBase
{
    [HttpGet]
    public async Task<List<PersonResponseDto>> Get()
    {
        return await personService.GetPersons();
    }

    [HttpGet("{id:long}")]
    public async Task<PersonResponseDto> Get(long id)
    {
        return await personService.GetPerson(id);
    }

    [HttpPost]
    public async Task<CreatePersonResponse> Post(PersonRequestDto person)
    {
        var personId = await personService.CreatePerson(person);
        return personId;
    }

    [HttpPut("{id:long}")]
    public async Task Put(long id, PersonRequestDto person)
    {
        await personService.UpdatePerson(id, person);
    }

    [HttpDelete("{id:long}")]
    public async Task Delete(long id)
    {
        await personService.DeletePerson(id);
    }
}