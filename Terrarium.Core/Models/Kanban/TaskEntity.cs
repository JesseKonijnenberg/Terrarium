using Terrarium.Core.Enums.Kanban;

namespace Terrarium.Core.Models.Kanban;

public class TaskEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; }
    public string Description { get; set; } = "";
    public string Tag { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime DueDate { get; set; }

    public string ColumnId { get; set; }
    public ColumnEntity Column { get; set; }
    public int Order { get; set; }
}