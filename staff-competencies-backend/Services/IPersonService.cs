using staff_competencies_backend.Dtos;
using staff_competencies_backend.Models;

namespace staff_competencies_backend.Services;

public interface IPersonService
{
    Task<List<PersonResponseDto>> GetPersons();
    Task<PersonResponseDto> GetPerson(long id);
    Task<CreatePersonResponse> CreatePerson(PersonRequestDto person);
    Task UpdatePerson(long id, PersonRequestDto dto);
    Task DeletePerson(long id);
}