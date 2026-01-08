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
            _updateMock.Object); 
    }

    private Column CreateTestColumn(string id, string title)
    {
        return new Column(new ColumnEntity { Id = id, Title = title });
    }

    [Fact]
    public void MoveTask_ShouldUpdateUICollectionsAndCallLogicService()
    {
        var vm = CreateViewModel();
        var colSource = CreateTestColumn("c1", "Backlog");
        var colTarget = CreateTestColumn("c2", "Done");
        var task = new TaskItem(new TaskEntity { Id = "t1" });
        
        colSource.Tasks.Add(task);
        vm.Columns.Add(colSource);
        vm.Columns.Add(colTarget);

        vm.MoveTask(task, colTarget);

        Assert.Empty(colSource.Tasks);
        Assert.Single(colTarget.Tasks);
        
        _boardServiceMock.Verify(s => s.MoveTasksWithEconomyAsync(
            It.Is<List<string>>(l => l.Contains("t1")), "c2", "Done", It.IsAny<int>()), Times.Once);
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
    public void OpenedTask_SettingNewTask_ShouldCallUpdateViaService()
    {
        var vm = CreateViewModel();
        var task1 = new TaskItem(new TaskEntity { Id = "t1", Title = "Original" });
        var task2 = new TaskItem(new TaskEntity { Id = "t2" });
        
        vm.OpenedTask = task1;
        task1.Title = "Updated";

        vm.OpenedTask = task2;

        _boardServiceMock.Verify(s => s.UpdateTaskFromUiAsync(
            task1.Entity, "Updated", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void ExecuteAddItem_ShouldRequestEntityFromServiceAndOpenPanel()
    {
        var vm = CreateViewModel();
        var col = CreateTestColumn("c1", "Backlog");
        vm.Columns.Add(col);
        
        var newEntity = new TaskEntity { Id = "new-id", ColumnId = "c1" };
        _boardServiceMock.Setup(s => s.CreateDefaultTaskEntity("c1")).ReturnsAsync(newEntity);

        vm.AddItemCommand.Execute(col);

        Assert.Single(col.Tasks);
        Assert.Equal("new-id", vm.OpenedTask.Id);
        _boardServiceMock.Verify(s => s.AddTaskAsync(newEntity, "c1"), Times.Once);
    }

    [Fact]
    public async Task ExecuteSmartPaste_ShouldProcessAndAddResultsToUI()
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
    public void ExecuteDeleteSelected_ShouldRemoveFromUIAndCallService()
    {
        var vm = CreateViewModel();
        var task = new TaskItem(new TaskEntity { Id = "del-me" });
        var col = CreateTestColumn("c1", "Backlog");
        col.Tasks.Add(task);
        vm.Columns.Add(col);
        
        vm.ToggleTaskSelectionCommand.Execute(task);
        vm.DeleteSelectedTasksCommand.Execute(null);
        
        Assert.Empty(col.Tasks);
        _boardServiceMock.Verify(s => s.DeleteMultipleTasksAsync(It.IsAny<List<string>>()), Times.Once);
    }

    [Fact]
    public void ExecuteDeselectAll_WhenPanelOpen_ShouldClosePanelFirst()
    {
        var vm = CreateViewModel();
        var task = new TaskItem(new TaskEntity { Id = "t1" });
        vm.OpenedTask = task;
        
        vm.DeselectAllCommand.Execute(null);
        
        Assert.Null(vm.OpenedTask);
        Assert.False(task.IsSelected);
    }
}