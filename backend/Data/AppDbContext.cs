using Microsoft.EntityFrameworkCore;

namespace SwedenStart;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Roadmap> Roadmaps => Set<Roadmap>();
    public DbSet<RoadmapTask> RoadmapTasks => Set<RoadmapTask>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<Roadmap>().ToTable("Roadmaps");
        modelBuilder.Entity<RoadmapTask>().ToTable("RoadmapTasks");
    }
}
