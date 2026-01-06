using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia.Input.Platform;
using Terrarium.Avalonia.Models.Kanban;
using Terrarium.Avalonia.ViewModels.Core;
using Terrarium.Core.Enums.Kanban;
using Terrarium.Core.Interfaces.Garden;
using Terrarium.Core.Interfaces.Kanban;
using Terrarium.Core.Models.Kanban;
using Terrarium.Logic.Services.Kanban;

namespace Terrarium.Avalonia.ViewModels
{
    public class KanbanBoardViewModel : ViewModelBase
    {

        private readonly IBoardService _boardService;
        private readonly IGardenEconomyService _gardenEconomyService;
        private readonly ITaskParserService _taskParserService;

        public UpdateViewModel Updater { get; } = new UpdateViewModel();
        public ICommand AddItemCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand SelectTaskCommand { get; }
        public ICommand SaveTaskCommand { get; }
        public ICommand SmartPasteCommand { get; }
        public ICommand SelectAllTasksCommand { get; }
        public ICommand DeleteAllTasksCommand { get; }
        public ICommand ConfirmDeleteAllCommand { get; }
        public ICommand CancelDeleteAllCommand { get; }

        public ObservableCollection<Column> Columns { get; set; } = new();

        public TaskItem? SelectedTask
        {
            get;
            set
            {
                if (field != null)
                {
                    SaveTask(field);
                }
                field = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDetailPanelOpen));
            }
        }
        private bool _isDeleteAllConfirmationOpen;
        public bool IsDeleteAllConfirmationOpen
        {
            get => _isDeleteAllConfirmationOpen;
            set { _isDeleteAllConfirmationOpen = value; OnPropertyChanged(); }
        }
        
        public bool IsDetailPanelOpen => SelectedTask != null;

        public KanbanBoardViewModel(
            IBoardService boardService, 
            IGardenEconomyService gardenEconomyService,
            ITaskParserService taskParserService)
        {
            _boardService = boardService;
            _gardenEconomyService = gardenEconomyService;
            _taskParserService = taskParserService;

            AddItemCommand = new RelayCommand(ExecuteAddItem);
            DeleteTaskCommand = new RelayCommand(ExecuteDeleteTask, CanExecuteDeleteTask);
            SelectTaskCommand = new RelayCommand(ExecuteSelectTask);
            SaveTaskCommand = new RelayCommand(ExecuteSaveTask);
            SmartPasteCommand = new RelayCommand(ExecuteSmartPaste);
            DeleteAllTasksCommand = new RelayCommand(_ => IsDeleteAllConfirmationOpen = true);
            ConfirmDeleteAllCommand = new RelayCommand(ExecuteConfirmDeleteAll);
            CancelDeleteAllCommand = new RelayCommand(_ => IsDeleteAllConfirmationOpen = false);
            
            LoadData();
        }

        public async void MoveTask(TaskItem task, Column targetColumn, int index = -1)
        {
            var sourceColumn = Columns.FirstOrDefault(c => c.Tasks.Contains(task));

            if (sourceColumn != null)
            {
                sourceColumn.Tasks.Remove(task);

                if (index == -1 || index > targetColumn.Tasks.Count)
                {
                    targetColumn.Tasks.Add(task);
                }
                else
                {
                    targetColumn.Tasks.Insert(index, task);
                }
                if (targetColumn.Title == "Complete" || targetColumn.Title == "Done")
                {
                    _gardenEconomyService.EarnWater(20);
                }

                await _boardService.MoveTaskAsync(task.Entity, targetColumn.Id, index);
            }
        }

        private void ExecuteSelectTask(object? parameter)
        {
            if (parameter is TaskItem task) SelectedTask = task;
        }

        public void CloseDetails()
        {
            if (SelectedTask != null)
            {
                SaveTask(SelectedTask);
            }
            SelectedTask = null;
        }

        private void ExecuteSaveTask(object? parameter)
        {
            if (SelectedTask != null)
            {
                SaveTask(SelectedTask);
            }
        }

        private async void SaveTask(TaskItem task)
        {
            task.Entity.Title = task.Title;
            task.Entity.Tag = task.Tag;
            task.Entity.Description = task.Description;

            if (Enum.TryParse(typeof(TaskPriority), task.Priority, true, out var result))
            {
                task.Entity.Priority = (TaskPriority)result;
            }
            else
            {
                task.Entity.Priority = TaskPriority.Low; // Default fallback
            }
            if (DateTime.TryParse(task.Date, out var dateResult))
            {
                task.Entity.DueDate = dateResult;
            }

            await _boardService.UpdateTaskAsync(task.Entity);
        }

        private async void ExecuteDeleteTask(object? parameter)
        {
            if (SelectedTask is TaskItem taskToDelete)
            {
                var sourceColumn = Columns.FirstOrDefault(c => c.Tasks.Contains(taskToDelete));
                if (sourceColumn != null)
                {
                    sourceColumn.Tasks.Remove(taskToDelete);
                    SelectedTask = null;

                    await _boardService.DeleteTaskAsync(taskToDelete.Entity);
                }
            }
        }

        private bool CanExecuteDeleteTask(object? parameter) => SelectedTask != null;

        private async void ExecuteAddItem(object? parameter)
        {
            if (parameter is Column targetColumn)
            {
                int nextId = 1;
                if (Columns.Any(c => c.Tasks.Any()))
                {
                    var allTasks = Columns.SelectMany(c => c.Tasks);
                    if (allTasks.Any())
                    {
                        nextId = allTasks.Max(t => int.TryParse(t.Id, out int id) ? id : 0) + 1;
                    }
                }

                var newEntity = new TaskEntity
                {
                    Id = nextId.ToString(),
                    Title = "New Task",
                    Tag = "New",
                    Description = "",
                    Priority = TaskPriority.Low,
                    DueDate = DateTime.Now
                };

                var newTask = new TaskItem(newEntity);
                targetColumn.Tasks.Insert(0, newTask);
                SelectedTask = newTask;

                await _boardService.AddTaskAsync(newEntity, targetColumn.Id);
            }
        }

        private async void LoadData()
        {
            var boardData = await ((BoardService)_boardService).LoadBoardAsync();

            Columns.Clear();
            foreach (var colEntity in boardData)
            {
                var columnVm = new Column(colEntity);
                foreach (var taskEntity in colEntity.Tasks)
                {
                    columnVm.Tasks.Add(new TaskItem(taskEntity));
                }
                Columns.Add(columnVm);
            }
        }
        
        private async void ExecuteConfirmDeleteAll(object? parameter)
        {
            IsDeleteAllConfirmationOpen = false; // Hide popup
    
            foreach (var column in Columns)
            {
                // Must clear DB first
                foreach (var task in column.Tasks.ToList())
                {
                    await _boardService.DeleteTaskAsync(task.Entity);
                }
                // Then clear UI
                column.Tasks.Clear();
            }
    
            SelectedTask = null;
            Console.WriteLine("[SYSTEM] Board wiped successfully.");
        }
        
        private async void ExecuteSmartPaste(object? parameter)
        {
            Console.WriteLine("--- SMART PASTE TRIGGERED ---");

            if (parameter == null)
            {
                Console.WriteLine("[ERROR] Command Parameter is NULL. The XAML binding is failing.");
                return;
            }

            if (parameter is not IClipboard clipboard)
            {
                Console.WriteLine($"[ERROR] Parameter is wrong type: {parameter.GetType().Name}. Expected: IClipboard.");
                return;
            }

            Console.WriteLine("[SUCCESS] Clipboard service found.");

            try 
            {
                var text = await clipboard.GetTextAsync();
                
                if (string.IsNullOrWhiteSpace(text))
                {
                    Console.WriteLine("[WARNING] Clipboard text is empty or null.");
                    return;
                }

                Console.WriteLine($"[INFO] Clipboard Content Found ({text.Length} chars):");
                Console.WriteLine(text);

                var parsedTasks = _taskParserService.ParseClipboardText(text).ToList();
                
                Console.WriteLine($"[INFO] Parser found {parsedTasks.Count} tasks.");

                if (!parsedTasks.Any()) 
                {
                    Console.WriteLine("[WARNING] Parser returned 0 tasks. Check Regex/Format.");
                    return;
                }

                var targetColumn = Columns.FirstOrDefault();
                if (targetColumn == null)
                {
                     Console.WriteLine("[ERROR] No columns found to paste into!");
                     return;
                }

                foreach (var entity in parsedTasks)
                {
                    Console.WriteLine($"[ACTION] Adding Task: {entity.Title}");
                    
                    int nextId = 1;
                    if (Columns.Any(c => c.Tasks.Any()))
                    {
                        nextId = Columns.SelectMany(c => c.Tasks)
                            .Max(t => int.TryParse(t.Id, out int id) ? id : 0) + 1;
                    }
                    entity.Id = nextId.ToString();

                    var newTask = new TaskItem(entity);
                    targetColumn.Tasks.Add(newTask);
                    await _boardService.AddTaskAsync(entity, targetColumn.Id);
                    
                    // Auto-select to confirm UI update
                    //SelectedTask = newTask;
                }
                Console.WriteLine("--- PASTE COMPLETE ---");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CRITICAL ERROR] {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}