using Microsoft.EntityFrameworkCore;
using Tracking.Domain.Entities;

namespace Tracking.Infrastructure.Data;

public class TrackingDbContext(DbContextOptions<TrackingDbContext> options) : DbContext(options)
{

    public DbSet<Project> Projects { get; set; } = default!;
    public DbSet<ProjectMember> ProjectMembers { get; set; } = default!;
    public DbSet<Issue> Issues { get; set; } = default!;

    public static string ImmutableUnaccent(string text) =>
        throw new NotSupportedException("Only for EF Core query translation.");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDbFunction(
            typeof(TrackingDbContext).GetMethod(nameof(ImmutableUnaccent), [typeof(string)])!)
            .HasName("immutable_unaccent");

        // Project
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Description).HasMaxLength(500);
        });

        // ProjectMember
        modelBuilder.Entity<ProjectMember>(entity =>
        {
            entity.HasKey(pm => pm.Id);

            entity.HasIndex(pm => new { pm.ProjectId, pm.UserId }).IsUnique();

            entity.HasOne(pm => pm.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(pm => pm.Role).HasConversion<string>().HasMaxLength(20);
        });

        // Issue
        modelBuilder.Entity<Issue>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Title).HasMaxLength(200).IsRequired();
            entity.Property(i => i.Description).HasMaxLength(2000);

            entity.HasOne(i => i.Project)
                .WithMany(p => p.Issues)
                .HasForeignKey(i => i.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(i => i.AssignedTo)
                .WithMany(pm => pm.AssignedIssues)
                .HasForeignKey(i => i.AssignedToId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(i => i.CreatedAt);
            entity.HasIndex(i => i.Status);
            entity.HasIndex(i => i.Priority);
            entity.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(i => i.Priority).HasConversion<string>().HasMaxLength(20);
        });
    }
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<Project>())
        {
            if (entry.State == EntityState.Added) entry.Entity.CreatedAt = now;
            if (entry.State is EntityState.Added or EntityState.Modified) entry.Entity.UpdatedAt = now;
        }
        foreach (var entry in ChangeTracker.Entries<Issue>())
        {
            if (entry.State == EntityState.Added) entry.Entity.CreatedAt = now;
            if (entry.State is EntityState.Added or EntityState.Modified) entry.Entity.UpdatedAt = now;
        }
        return await base.SaveChangesAsync(cancellationToken);
    }

}
