using System.Net.Http.Json;
using VisualizePeopleMCP.Entities;

namespace VisualizePeopleMCP.Services
{
    public class PeopleServiceHTTP(HttpClient httpClient) : IPeopleService
    {
        public async Task<IEnumerable<Person>> GetAll()
        {
            var url = "https://mcpserver20260506141716-a8due4dabgfxb0fy.eastus-01.azurewebsites.net/api/people";
            var result = await httpClient.GetFromJsonAsync<IEnumerable<Person>>(url);
            return result!;
        }
    }
}
