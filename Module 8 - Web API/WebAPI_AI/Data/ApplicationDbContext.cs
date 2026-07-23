using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebAPI_AI.Entities;

namespace WebAPI_AI.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        public DbSet<ChatConversation> Conversations { get; set; }
        public DbSet<ChatMessage> Messages { get; set; }
        public DbSet<ChatApprovalRequest> ApprovalRequests { get; set; }
    }
}
