using Logging.Domain.Enums;

namespace Logging.Domain.Entities;

public class LogChange
{
    public Guid Id { get; set; }
    public Guid ActivityLogId { get; private set; }
    public required string Field { get; init; }
    public ChangeValueType? Type { get; init; }
    public string? OldValue { get; init; }
    public string? OldCode { get; init; }
    public string? OldId { get; init; }
    public string? NewValue { get; init; }
    public string? NewCode { get; init; }
    public string? NewId { get; init; }
    public ActivityLog? ActivityLog { get; set; }
}