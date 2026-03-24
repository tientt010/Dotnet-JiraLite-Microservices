using Logging.Domain.Enums;

namespace Logging.Domain.Entities;

public record LogTarget(
    TargetType Type,
    string Id,
    string Code,
    string Name
);