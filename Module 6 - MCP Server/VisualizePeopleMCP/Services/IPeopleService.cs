using VisualizePeopleMCP.Entities;

namespace VisualizePeopleMCP.Services
{
    public interface IPeopleService
    {
        Task<IEnumerable<Person>> GetAll();
    }
}
