using MCPServer.Entities;

namespace MCPServer.Services
{
    public interface IPeopleRepository
    {
        bool UpdateActive(int id, bool active);
        Person? GetById(int id);
        List<Person> GetAll();
    }
}
