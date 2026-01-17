using Terrarium.Core.Enums.Kanban;

namespace Terrarium.Core.Models.Kanban.DTO;

public record ParsedTaskResult(
    string Title,
    string Description,
    string Tag,
    TaskPriority Priority,
    string TargetColumnName
);