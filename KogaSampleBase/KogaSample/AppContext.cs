using System.Data.Entity;

namespace KogaSample
{
    public class AppContext : DbContext
    {
        public AppContext(): base("SqLiteConnection")
        {}

        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Worker> Workers { get; set; }
        public DbSet<OrganizationWorker> OrganizationWorkers { get; set; }
    }
}
