using System.Collections.Generic;

namespace Terrarium.Core.Models.Kanban
{
    public class ColumnEntity
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public List<TaskEntity> Tasks { get; set; } = new();
    }
}