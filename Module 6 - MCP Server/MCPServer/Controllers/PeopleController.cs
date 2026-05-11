using MCPServer.Entities;
using MCPServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace MCPServer.Controllers
{
    [ApiController]
    [Route("api/people")]
    public class PeopleController(IPeopleRepository peopleRepository): ControllerBase
    {
        [HttpGet]
        public List<Person> GetAll()
        {
            return peopleRepository.GetAll();
        }
    }
}
