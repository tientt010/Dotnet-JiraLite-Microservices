using System;
using JiraLite.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace JiraLite.Infrastructure.Data;

public class JiraLiteDbContext(DbContextOptions<JiraLiteDbContext> options) : DbContext(options)
{

    public DbSet<Project> Projects { get; set; } = default!;
    public DbSet<ProjectMember> ProjectMembers { get; set; } = default!;
    public DbSet<Issue> Issues { get; set; } = default!;
    public DbSet<IssueChangeLog> IssueChangeLogs { get; set; } = default!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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

            entity.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(i => i.Priority).HasConversion<string>().HasMaxLength(20);
        });

        // IssueChangeLog
        modelBuilder.Entity<IssueChangeLog>(entity =>
        {
            entity.HasKey(log => log.Id);

            entity.HasOne(log => log.Issue)
                .WithMany(i => i.ChangeLogs)
                .HasForeignKey(log => log.IssueId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(log => log.ChangedBy)
                .WithMany()
                .HasForeignKey(log => log.ChangedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(log => log.ChangeType).HasConversion<string>().HasMaxLength(30);
            entity.Property(log => log.OldValue).HasMaxLength(500);
            entity.Property(log => log.NewValue).HasMaxLength(500);
            entity.Property(log => log.Description).HasMaxLength(500);
        });
    }

}
