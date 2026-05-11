using MCPServer.Entities;

namespace MCPServer.Services
{
    public class PeopleRepositoryInMemory : IPeopleRepository
    {

        private List<Person> _people;

        public PeopleRepositoryInMemory()
        {
            _people = new List<Person>
        {
            new Person
            {
                Id = 1,
                Name = "Felipe Gavilán",
                Email = "Felipe.Gavilan@email.com",
                Salary = 50000,
                Active = true
            },
            new Person
            {
                Id = 2,
                Name = "Claudia Rodríguez",
                Email = "claudia.rodriguez@email.com",
                Salary = 65000,
                Active = true
            },
            new Person
            {
                Id = 3,
                Name = "Carlos Rodríguez",
                Email = "carlos.rodriguez@email.com",
                Salary = 45000,
                Active = false
            }
        };

        }

        public List<Person> GetAll()
        {
            return _people;
        }

        public Person? GetById(int id)
        {
            return _people.FirstOrDefault(p => p.Id == id);
        }

        public bool UpdateActive(int id, bool active)
        {
            var person = _people.FirstOrDefault(p => p.Id == id);

            if (person is null)
            {
                return false;
            }

            person.Active = active;
            return true;
        }
    }
}
