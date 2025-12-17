using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terrarium.Core.Enums;
using Terrarium.Core.Interfaces;
using Terrarium.Core.Models;

namespace Terrarium.Logic.Services
{
    public class BoardService : IBoardService
    {
        private List<ColumnEntity> _board;

        public BoardService()
        {
            _board = GenerateDummyData();
        }

        public List<ColumnEntity> GetFullBoard()
        {
            return _board;
        }


        public Task AddTaskAsync(TaskEntity task, string columnId)
        {
            return Task.Delay(50);
        }

        public Task MoveTaskAsync(TaskEntity task, string targetColumnId, int index)
        {
            return Task.Delay(50);
        }

        public Task DeleteTaskAsync(TaskEntity task)
        {
            return Task.Delay(50);
        }

        private List<ColumnEntity> GenerateDummyData()
        {
            TaskEntity Create(string id, string content, string tag, TaskPriority priority, int days)
            {
                return new TaskEntity { Id = id, Content = content, Tag = tag, Priority = priority, DueDate = DateTime.Now.AddDays(days) };
            }

            return new List<ColumnEntity>
            {
                new ColumnEntity
                {
                    Id = "col-1", Title = "Backlog",
                    Tasks = new List<TaskEntity> { Create("1", "Design Sys", "Design", TaskPriority.High, 1) }
                },
                new ColumnEntity
                {
                    Id = "col-2", Title = "In Progress",
                    Tasks = new List<TaskEntity> { Create("4", "Dark Mode", "Dev", TaskPriority.High, 0) }
                },
                new ColumnEntity { Id = "col-3", Title = "Review", Tasks = new List<TaskEntity>() },
                new ColumnEntity { Id = "col-4", Title = "Complete", Tasks = new List<TaskEntity>() }
            };
        }
    }
}