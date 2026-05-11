using BlazorAI.Data;
using BlazorAI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorAI.Services
{
    public class PersonService(IDbContextFactory<ApplicationDbContext> dbContextFactory) : IPersonService
    {
        public async Task<IEnumerable<Person>> GetAll()
        {
            using var context = dbContextFactory.CreateDbContext();
            return await context.People.ToListAsync();
        }
    }
}
