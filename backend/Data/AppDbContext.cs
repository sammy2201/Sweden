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

    public DbSet<Attraction> Attractions => Set<Attraction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<Roadmap>().ToTable("Roadmaps");
        modelBuilder.Entity<RoadmapTask>().ToTable("RoadmapTasks");
        modelBuilder.Entity<Attraction>().ToTable("Attractions");

        modelBuilder.Entity<Roadmap>()
            .HasIndex(r => r.UserId)
            .IsUnique();

        modelBuilder.Entity<Roadmap>()
            .HasOne(r => r.User)
            .WithMany(u => u.Roadmaps)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Roadmap>()
            .HasMany(r => r.Tasks)
            .WithOne(t => t.Roadmap)
            .HasForeignKey(t => t.RoadmapId);
    }
}
