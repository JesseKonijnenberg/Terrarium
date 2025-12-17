using System;
using System.Collections.Generic;
using Terrarium.Core.Enums;
using Terrarium.Core.Interfaces;
using Terrarium.Core.Models;

namespace Terrarium.Logic.Services
{

    public class BoardService : IBoardService
    {

        public void AddTaskToColumn(ColumnEntity column, TaskEntity task)
        {

            if (!column.Tasks.Contains(task))
            {
                column.Tasks.Add(task);
                // SaveToDatabase(column);
            }
        }

        public void RemoveTaskFromColumn(ColumnEntity column, TaskEntity task)
        {
            if (column.Tasks.Contains(task))
            {
                column.Tasks.Remove(task);
                // SaveToDatabase(column);
            }
        }

        public List<ColumnEntity> GetFullBoard()
        {
            TaskEntity CreateTask(string id, string content, string tag, TaskPriority priority, int daysFromNow)
            {
                return new TaskEntity
                {
                    Id = id,
                    Content = content,
                    Tag = tag,
                    Priority = priority,
                    DueDate = DateTime.Now.AddDays(daysFromNow)
                };
            }

            return new List<ColumnEntity>
            {
                new ColumnEntity
                {
                    Id = "col-1",
                    Title = "Backlog",
                    Tasks = new List<TaskEntity>
                    {
                        CreateTask("1", "Design System Audit", "Design", TaskPriority.High, 1),
                        CreateTask("2", "Q3 Marketing Assets", "Marketing", TaskPriority.Medium, 20),
                        CreateTask("3", "Update dependencies", "Dev", TaskPriority.Low, 24)
                    }
                },
                new ColumnEntity
                {
                    Id = "col-2",
                    Title = "In Progress",
                    Tasks = new List<TaskEntity>
                    {
                        CreateTask("4", "Dark Mode Implementation", "Dev", TaskPriority.High, 0)
                    }
                },
                new ColumnEntity
                {
                    Id = "col-3",
                    Title = "Review",
                    Tasks = new List<TaskEntity>()
                },
                new ColumnEntity
                {
                    Id = "col-4",
                    Title = "Complete",
                    Tasks = new List<TaskEntity>
                    {
                        CreateTask("5", "Competitor Analysis", "Product", TaskPriority.Medium, -5)
                    }
                }
            };
        }
    }
}