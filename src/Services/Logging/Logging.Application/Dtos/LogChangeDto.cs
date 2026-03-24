using System;

namespace Logging.Application.Dtos;

public record class LogChangeDto
{
    public string Field { get; init; } = string.Empty;
    public string? OldValue { get; init; }
    public string? OldCode { get; init; }
    public string? OldId { get; init; }
    public string? NewValue { get; init; }
    public string? NewCode { get; init; }
    public string? NewId { get; init; }
}
