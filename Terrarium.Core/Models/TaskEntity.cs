using System;
using Terrarium.Core.Enums;

namespace Terrarium.Core.Models
{
    public class TaskEntity
    {
        public string Id { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Tag { get; set; } = "General";
        public TaskPriority Priority { get; set; } = TaskPriority.Low;
        public DateTime DueDate { get; set; } = DateTime.Now;
    }
}