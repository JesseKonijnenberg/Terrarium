using Avalonia.Input.Platform;
using Moq;
using Terrarium.Avalonia.Models.Kanban;
using Terrarium.Avalonia.ViewModels;
using Terrarium.Core.Interfaces.Garden;
using Terrarium.Core.Interfaces.Kanban;
using Terrarium.Core.Interfaces.Update;
using Terrarium.Core.Models.Kanban;

namespace Terrarium.Tests.Avalonia.ViewModels;

public class KanbanBoardViewModelTests
{
    private readonly Mock<IBoardService> _boardServiceMock;
    private readonly Mock<IGardenEconomyService> _economyMock;
    private readonly Mock<ITaskParserService> _parserMock;
    private readonly Mock<IUpdateService> _updateMock;

    public KanbanBoardViewModelTests()
    {
        _boardServiceMock = new Mock<IBoardService>();
        _economyMock = new Mock<IGardenEconomyService>();
        _parserMock = new Mock<ITaskParserService>();
        _updateMock = new Mock<IUpdateService>();
    }

    private KanbanBoardViewModel CreateViewModel()
    {
        _boardServiceMock.Setup(s => s.LoadBoardAsync()).ReturnsAsync(new List<ColumnEntity>());
        
        return new KanbanBoardViewModel(
            _boardServiceMock.Object, 
            _economyMock.Object, 
            _parserMock.Object,
            _updateMock.Object); 
    }

    private Column CreateTestColumn(string id, string title)
    {
        return new Column(new ColumnEntity { Id = id, Title = title });
    }

    [Fact]
    public void MoveTask_SingleTask_ShouldUpdateCollectionsAndCallService()
    {
        var vm = CreateViewModel();
        var colSource = CreateTestColumn("c1", "Backlog");
        var colTarget = CreateTestColumn("c2", "In Progress");
        var task = new TaskItem(new TaskEntity { Id = "t1" });
        
        colSource.Tasks.Add(task);
        vm.Columns.Add(colSource);
        vm.Columns.Add(colTarget);

        vm.MoveTask(task, colTarget);

        Assert.Empty(colSource.Tasks);
        Assert.Single(colTarget.Tasks);
        _boardServiceMock.Verify(s => s.MoveMultipleTasksAsync(
            It.Is<List<string>>(l => l.Contains("t1")), "c2", It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public void MoveTask_ToDoneColumn_ShouldEarnWaterReward()
    {
        var vm = CreateViewModel();
        var colDone = CreateTestColumn("c-done", "Done");
        var task = new TaskItem(new TaskEntity { Id = "t1" });
        vm.Columns.Add(colDone);

        vm.MoveTask(task, colDone);

        _economyMock.Verify(e => e.EarnWater(20), Times.Once);
    }

    [Fact]
    public void ExecuteOpenTask_ShouldSetOpenedTaskAndSelectIt()
    {
        var vm = CreateViewModel();
        var task = new TaskItem(new TaskEntity { Id = "t1" });

        vm.OpenTaskCommand.Execute(task);

        Assert.Equal(task, vm.OpenedTask);
        Assert.Contains("t1", vm.SelectedTaskIds);
    }

    [Fact]
    public void OpenedTask_SettingNewTask_ShouldTriggerAutoSaveOnOldTask()
    {
        var vm = CreateViewModel();
        var task1 = new TaskItem(new TaskEntity { Id = "t1", Title = "Original" });
        var task2 = new TaskItem(new TaskEntity { Id = "t2" });
        
        vm.OpenedTask = task1;
        task1.Title = "Updated";

        vm.OpenedTask = task2;

        _boardServiceMock.Verify(s => s.UpdateTaskAsync(It.Is<TaskEntity>(t => t.Title == "Updated")), Times.Once);
    }

    [Fact]
    public void ExecuteAddItem_ShouldInsertAtTopAndOpenPanel()
    {
        var vm = CreateViewModel();
        var col = CreateTestColumn("c1", "Backlog");
        vm.Columns.Add(col);

        vm.AddItemCommand.Execute(col);

        Assert.Single(col.Tasks);
        Assert.Equal(col.Tasks[0], vm.OpenedTask);
        _boardServiceMock.Verify(s => s.AddTaskAsync(It.IsAny<TaskEntity>(), "c1"), Times.Once);
    }

    [Fact]
    public async Task ExecuteSmartPaste_ShouldAddTasksToCorrectUIColumns()
    {
        var vm = CreateViewModel();
        var col = CreateTestColumn("target-col", "Target");
        vm.Columns.Add(col);
        
        var clipboardMock = new Mock<IClipboard>();
        clipboardMock.Setup(c => c.GetTextAsync()).ReturnsAsync("markdown");
        
        var parsedEntities = new List<TaskEntity> { new TaskEntity { Id = "p1", ColumnId = "target-col" } };
        _boardServiceMock.Setup(s => s.ProcessSmartPasteAsync(It.IsAny<string>())).ReturnsAsync(parsedEntities);

        vm.SmartPasteCommand.Execute(clipboardMock.Object);
        await Task.Delay(50);

        Assert.Single(col.Tasks);
        Assert.Equal("p1", col.Tasks[0].Id);
    }

    [Fact]
    public void ExecuteDeleteSelected_ShouldWipeFromUIAndClosePanel()
    {
        var vm = CreateViewModel();
        var task = new TaskItem(new TaskEntity { Id = "del-me" });
        var col = CreateTestColumn("c1", "Backlog");
        col.Tasks.Add(task);
        vm.Columns.Add(col);
        
        vm.ToggleTaskSelectionCommand.Execute(task);
        vm.OpenedTask = task;

        vm.DeleteSelectedTasksCommand.Execute(null);

        Assert.Empty(col.Tasks);
        Assert.Null(vm.OpenedTask);
        Assert.Empty(vm.SelectedTaskIds);
        _boardServiceMock.Verify(s => s.DeleteMultipleTasksAsync(It.IsAny<List<string>>()), Times.Once);
    }

    [Fact]
    public void ExecuteDeselectAll_ShouldClearSelectionAndClosePanel()
    {
        var vm = CreateViewModel();
        var col = CreateTestColumn("c1", "Backlog");
        var task = new TaskItem(new TaskEntity { Id = "t1" });
        
        col.Tasks.Add(task);
        vm.Columns.Add(col);

        task.IsSelected = true;
        vm.SelectedTaskIds.Add("t1");
        vm.OpenedTask = task;
        
        vm.DeselectAllCommand.Execute(null);
        
        Assert.False(task.IsSelected);
        Assert.Empty(vm.SelectedTaskIds);
        Assert.Null(vm.OpenedTask);
    }
}