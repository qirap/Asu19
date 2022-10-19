using Microsoft.EntityFrameworkCore;

namespace Asu19.Database
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Users> Users { get; set; } = null!;
        public DbSet<Cars> Cars { get; set; } = null!;
        public DbSet<Employees> Employees { get; set; } = null!;
        public DbSet<Services> Services { get; set; } = null!;
        public DbSet<UserCar> UserCar { get; set; } = null!;
        public DbSet<EmployeeService> EmployeeService { get; set; } = null!;
        public DbSet<Requests> Requests { get; set; } = null!;

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            Database.EnsureCreated();
        }
    }
}
