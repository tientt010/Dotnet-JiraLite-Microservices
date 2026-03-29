using System;
using Comment.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Comment.Infrastructure.Data;

public class CommentDbContext(DbContextOptions<CommentDbContext> options) : DbContext(options), ICommentReadDbContext
{
    public DbSet<Domain.Entities.Comment> Comments => Set<Domain.Entities.Comment>();
    IQueryable<Domain.Entities.Comment> ICommentReadDbContext.Comments => Comments.AsNoTracking();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Domain.Entities.Comment>(comment =>
        {
            comment.ToTable("Comments");

            comment.HasKey(x => x.Id);
            comment.Property(x => x.Id)
                .HasDefaultValueSql("gen_random_uuid()");


            comment.Property(x => x.ProjectId)
                .IsRequired();

            comment.Property(x => x.IssueId)
                .IsRequired();

            comment.Property(x => x.AuthorId)
                .IsRequired();

            comment.Property(x => x.AuthorCode)
                .IsRequired()
                .HasMaxLength(100);

            comment.Property(x => x.AuthorName)
                .IsRequired()
                .HasMaxLength(255);

            comment.Property(x => x.AuthorAvatarUrl)
                .HasMaxLength(500);

            comment.Property(x => x.Content)
                .IsRequired()
                .HasMaxLength(4000);

            comment.Property(x => x.CreatedAt).IsRequired();
            comment.Property(x => x.UpdatedAt).IsRequired();

            comment.HasOne(x => x.ParentComment)
                .WithMany(x => x.Replies)
                .HasForeignKey("ParentCommentId")
                .OnDelete(DeleteBehavior.Restrict);

            comment.HasQueryFilter(c => c.DeletedAt == null);

            // Composite index với filter
            comment.HasIndex(x => new { x.ProjectId, x.IssueId })
                .HasDatabaseName("IX_Comments_ProjectId_IssueId_Active")
                .HasFilter("\"DeletedAt\" IS NULL");  // Partial index

            comment.HasIndex(x => x.AuthorId)
                .HasDatabaseName("IX_Comments_AuthorId_Active")
                .HasFilter("\"DeletedAt\" IS NULL");

            comment.HasIndex(x => x.ParentCommentId)
                .HasDatabaseName("IX_Comments_ParentCommentId_Active")
                .HasFilter("\"DeletedAt\" IS NULL");
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<Domain.Entities.Comment>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

}
