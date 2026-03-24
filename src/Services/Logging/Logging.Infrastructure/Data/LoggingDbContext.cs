using Logging.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Logging.Application.Interfaces;

namespace Logging.Infrastructure.Data;

public class LoggingDbContext : DbContext, ILoggingReadDbContext
{
    public LoggingDbContext(DbContextOptions<LoggingDbContext> options) : base(options)
    {
    }

    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<LogChange> LogChanges => Set<LogChange>();

    IQueryable<ActivityLog> ILoggingReadDbContext.ActivityLogs => ActivityLogs.AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var activityLog = modelBuilder.Entity<ActivityLog>();

        activityLog.ToTable("ActivityLogs");

        activityLog.HasKey(x => x.Id);
        activityLog.Property(x => x.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        activityLog.Property(x => x.Timestamp)
            .IsRequired()
            .HasConversion(
                v => v.UtcDateTime,
                v => new DateTimeOffset(v, TimeSpan.Zero));

        activityLog.Property(x => x.ActionType)
            .IsRequired()
            .HasConversion<string>();

        activityLog.OwnsOne(x => x.Actor, actor =>
        {
            actor.Property(a => a.Id)
                .HasColumnName("ActorId")
                .HasMaxLength(450)
                .IsRequired();

            actor.Property(a => a.Code)
                .HasColumnName("ActorCode")
                .HasMaxLength(100)
                .IsRequired();

            actor.Property(a => a.Name)
                .HasColumnName("ActorName")
                .HasMaxLength(255)
                .IsRequired();

            actor.Property(a => a.AvatarUrl)
                .HasColumnName("ActorAvatarUrl")
                .HasMaxLength(2048);

            // Index on ActorId
            actor.HasIndex(a => a.Id)
                .HasDatabaseName("IX_ActivityLogs_ActorId");
        });

        activityLog.OwnsOne(x => x.Target, target =>
        {
            target.Property(t => t.Type)
                .HasColumnName("TargetType")
                .IsRequired()
                .HasConversion<string>();

            target.Property(t => t.Id)
                .HasColumnName("TargetId")
                .HasMaxLength(450)
                .IsRequired();

            target.Property(t => t.Code)
                .HasColumnName("TargetCode")
                .HasMaxLength(100)
                .IsRequired();

            target.Property(t => t.Name)
                .HasColumnName("TargetName")
                .HasMaxLength(255)
                .IsRequired();

            // Index on TargetId
            target.HasIndex(t => t.Id)
                .HasDatabaseName("IX_ActivityLogs_TargetId");
        });

        activityLog.HasMany(x => x.Changes)
            .WithOne(x => x.ActivityLog)
            .HasForeignKey(x => x.ActivityLogId)
            .OnDelete(DeleteBehavior.Cascade);

        activityLog.HasIndex(x => x.Timestamp)
            .HasDatabaseName("IX_ActivityLogs_Timestamp")
            .IsDescending(true);

        var logChange = modelBuilder.Entity<LogChange>();

        logChange.ToTable("LogChanges");

        logChange.HasKey(x => x.Id);
        logChange.Property(x => x.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        logChange.Property(x => x.ActivityLogId)
            .IsRequired();

        logChange.Property(x => x.Field)
            .HasMaxLength(100)
            .IsRequired();

        logChange.Property(x => x.Type)
            .HasConversion<string?>()
            .HasColumnName("Type");

        logChange.Property(x => x.OldValue)
            .HasMaxLength(500);

        logChange.Property(x => x.OldCode)
            .HasMaxLength(100);

        logChange.Property(x => x.OldId)
            .HasMaxLength(450);

        logChange.Property(x => x.NewValue)
            .HasMaxLength(500);

        logChange.Property(x => x.NewCode)
            .HasMaxLength(100);

        logChange.Property(x => x.NewId)
            .HasMaxLength(450);

        logChange.HasIndex(x => x.ActivityLogId)
            .HasDatabaseName("IX_LogChanges_ActivityLogId");

        logChange.HasIndex(x => new { x.Field, x.OldId })
            .HasDatabaseName("IX_LogChanges_Field_OldId")
            .HasFilter("\"OldId\" IS NOT NULL");

        logChange.HasIndex(x => new { x.Field, x.NewId })
            .HasDatabaseName("IX_LogChanges_Field_NewId")
            .HasFilter("\"NewId\" IS NOT NULL");
    }
}
