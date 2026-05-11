using MCPServer.DTOs;
using MCPServer.Entities;
using MCPServer.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MCPServer.Tools
{
    [McpServerToolType]
    public class PeopleTools(IPeopleRepository peopleRepository)
    {
        [McpServerTool, Description("Gets the list of all registered people.")]
        public List<Person> GetAll()
        {
            var people = peopleRepository.GetAll();
            return people;
        }

        [McpServerTool, Description("Gets a person by their identifier.")]
        public Person? GetById(
        [Description("Unique identifier of the person.")] int id)
        {
            var person = peopleRepository.GetById(id);
            return person;
        }

        [McpServerTool, Description("Activates or deactivates a person based on their identifier.")]
        public OperationResultDTO UpdateActive(
        [Description("Identifier of the person.")] int id,
        [Description("Indicates whether the person will be active (true) or inactive (false).")] bool active)
        {
            var updated = peopleRepository.UpdateActive(id, active);

            if (!updated)
            {
                return new OperationResultDTO(false, $"Could not update the person with id {id}. Please verify that the person exists.");
            }

            return new OperationResultDTO(true, "The update was completed successfully");
        }
    }
}
