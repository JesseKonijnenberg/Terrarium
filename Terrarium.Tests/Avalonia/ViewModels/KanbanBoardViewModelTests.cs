using Avalonia.Input.Platform;
using Moq;
using Terrarium.Avalonia.Models.Kanban;
using Terrarium.Avalonia.ViewModels;
using Terrarium.Core.Interfaces.Kanban;
using Terrarium.Core.Interfaces.Update;
using Terrarium.Core.Models.Kanban;

namespace Terrarium.Tests.Avalonia.ViewModels;

public class KanbanBoardViewModelTests
{
    private readonly Mock<IBoardService> _boardServiceMock;
    private readonly Mock<IUpdateService> _updateMock;
    private const string DefaultWorkspaceId = "solo-workspace";

    public KanbanBoardViewModelTests()
    {
        _boardServiceMock = new Mock<IBoardService>();
        _updateMock = new Mock<IUpdateService>();
    }

    private KanbanBoardViewModel CreateViewModel()
    {
        _boardServiceMock.Setup(s => s.LoadBoardAsync(DefaultWorkspaceId, It.IsAny<string>()))
            .ReturnsAsync(new List<ColumnEntity>());
        
        return new KanbanBoardViewModel(
            _boardServiceMock.Object,
            _updateMock.Object); 
    }

    private Column CreateTestColumn(string id, string title)
    {
        return new Column(new ColumnEntity { Id = id, Title = title });
    }

    [Fact]
    public void ExecuteAddItem_ShouldRequestEntityWithScopeAndOpenPanel()
    {
        var vm = CreateViewModel();
        var col = CreateTestColumn("c1", "Backlog");
        vm.Columns.Add(col);
        
        var newEntity = new TaskEntity { Id = "new-id", ColumnId = "c1" };
        
        _boardServiceMock.Setup(s => s.CreateDefaultTaskEntity("c1", DefaultWorkspaceId, null))
            .ReturnsAsync(newEntity);

        vm.AddItemCommand.Execute(col);

        Assert.Single(col.Tasks);
        Assert.Equal("new-id", vm.OpenedTask!.Id);
        
        _boardServiceMock.Verify(s => s.AddTaskAsync(newEntity, "c1", DefaultWorkspaceId, null), Times.Once);
    }

    [Fact]
    public async Task ExecuteSmartPaste_ShouldProcessWithScopeAndAddResults()
    {
        var vm = CreateViewModel();
        var col = CreateTestColumn("target-col", "Target");
        vm.Columns.Add(col);
        
        var clipboardMock = new Mock<IClipboard>();
        clipboardMock.Setup(c => c.GetTextAsync()).ReturnsAsync("markdown task");
        
        var parsedEntities = new List<TaskEntity> { new TaskEntity { Id = "p1", ColumnId = "target-col" } };
        
        _boardServiceMock.Setup(s => s.ProcessSmartPasteAsync("markdown task", DefaultWorkspaceId, null))
            .ReturnsAsync(parsedEntities);

        await vm.SmartPasteCommand.ExecuteAsync(clipboardMock.Object);

        Assert.Single(col.Tasks);
        Assert.Equal("p1", col.Tasks[0].Id);
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
    public async Task ChangeWorkspace_ShouldTriggerReload()
    {
        var vm = CreateViewModel();
        
        vm.CurrentWorkspaceId = "new-dept-id";
        
        _boardServiceMock.Verify(s => s.LoadBoardAsync("new-dept-id", It.IsAny<string>()), Times.AtLeastOnce());
    }

    [Fact]
    public async Task MoveTask_ShouldCallServiceWithCorrectParameters()
    {
        var vm = CreateViewModel();
        var colSource = CreateTestColumn("c1", "Backlog");
        var colTarget = CreateTestColumn("c2", "Done");
        var task = new TaskItem(new TaskEntity { Id = "t1" });
        
        colSource.Tasks.Add(task);
        vm.Columns.Add(colSource);
        vm.Columns.Add(colTarget);

        await vm.MoveTaskAsync(task, colTarget);

        _boardServiceMock.Verify(s => s.MoveTasksWithEconomyAsync(
            It.Is<List<string>>(l => l.Contains("t1")), "c2", "Done", It.IsAny<int>()), Times.Once);
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