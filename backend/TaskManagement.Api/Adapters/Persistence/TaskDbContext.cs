using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Core.Domain;

namespace TaskManagement.Api.Adapters.Persistence;

public class TaskDbContext : DbContext
{
    public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options)
    {
    }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("Tasks");

            entity.HasKey(t => t.Id);

            entity.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(t => t.IsCompleted)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(t => t.CreatedAt)
                .IsRequired();

            // Supports a future "filter by status" feature without a table scan.
            entity.HasIndex(t => t.IsCompleted);

            // Seed data, baked into the InitialCreate migration below so a fresh
            // `dotnet ef database update` gives every reviewer the same starting data.
            entity.HasData(
                new TaskItem
                {
                    Id = SeedIds.DesignSchema,
                    Title = "Design the database schema",
                    IsCompleted = true,
                    CreatedAt = new DateTime(2026, 7, 10, 9, 0, 0, DateTimeKind.Utc)
                },
                new TaskItem
                {
                    Id = SeedIds.BuildApi,
                    Title = "Build the REST API",
                    IsCompleted = true,
                    CreatedAt = new DateTime(2026, 7, 11, 9, 0, 0, DateTimeKind.Utc)
                },
                new TaskItem
                {
                    Id = SeedIds.WireUpFrontend,
                    Title = "Wire up the React frontend",
                    IsCompleted = false,
                    CreatedAt = new DateTime(2026, 7, 12, 9, 0, 0, DateTimeKind.Utc)
                });
        });
    }

    /// <summary>
    /// Fixed GUIDs for seed rows. HasData requires deterministic keys (it diffs
    /// seed data between migrations by key), so these cannot be Guid.NewGuid().
    /// </summary>
    public static class SeedIds
    {
        public static readonly Guid DesignSchema = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public static readonly Guid BuildApi = Guid.Parse("22222222-2222-2222-2222-222222222222");
        public static readonly Guid WireUpFrontend = Guid.Parse("33333333-3333-3333-3333-333333333333");
    }
}
