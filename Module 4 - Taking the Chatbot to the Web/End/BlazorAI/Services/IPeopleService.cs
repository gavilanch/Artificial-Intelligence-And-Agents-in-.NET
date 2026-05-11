using BlazorAI.Entities;
using System.ComponentModel;

namespace BlazorAI.Services
{
    [Description("Service to interact with people.")]
    public interface IPersonService
    {
        [Description("Gets a list of all people.")]
        Task<IEnumerable<Person>> GetAll();
    }
}
