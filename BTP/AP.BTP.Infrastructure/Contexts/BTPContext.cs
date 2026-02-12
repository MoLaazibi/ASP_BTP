using AP.BTP.Domain;
using AP.BTP.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AP.BTP.Infrastructure.Contexts
{
    public class BTPContext : DbContext
    {
        public BTPContext(DbContextOptions<BTPContext> options) : base(options)
        {
        }

        public BTPContext()
        {
        }

        public DbSet<NurserySite> NurserySites { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<EmployeeTask> EmployeeTask { get; set; }
        public DbSet<TaskList> TaskList { get; set; }
        public DbSet<GroundPlan> GroundPlans { get; set; }
        public DbSet<User> UserList { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Zone> Zones { get; set; }
        public DbSet<TreeType> TreeTypes { get; set; }
        public DbSet<TreeImage> TreeImages { get; set; }
        public DbSet<Instruction> Instructions { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            modelBuilder.Entity<EmployeeTask>().Seed();
            modelBuilder.Entity<TaskList>().Seed();
            modelBuilder.Entity<TreeType>().Seed();
            modelBuilder.Entity<User>().Seed();
            modelBuilder.Entity<UserRole>().Seed();
            modelBuilder.Entity<Address>().Seed();
            modelBuilder.Entity<NurserySite>().Seed();
            modelBuilder.Entity<Zone>().Seed();
            modelBuilder.Entity<GroundPlan>().Seed();
            modelBuilder.Entity<Category>().Seed();
        }

        // Add this method for design-time
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // This is only used for design-time (migrations)
                optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=BTPdb;Trusted_Connection=true;TrustServerCertificate=true;");
            }
        }
    }
}
