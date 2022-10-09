using Microsoft.EntityFrameworkCore;

namespace Asu19.Database
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Users> Users { get; set; } = null!;
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
