using Moq;
using Terrarium.Core.Enums.Kanban;
using Terrarium.Core.Interfaces.Garden;
using Terrarium.Core.Interfaces.Kanban;
using Terrarium.Core.Models.Kanban;
using Terrarium.Core.Models.Kanban.DTO;
using Terrarium.Logic.Services.Kanban;

namespace Terrarium.Tests.Logic.Services.Kanban;

public class BoardServiceTests
{
    private readonly Mock<IBoardRepository> _repoMock;
    private readonly Mock<ITaskParserService> _parserMock;
    private readonly Mock<IGardenEconomyService> _economyMock;
    private readonly BoardService _service;

    public BoardServiceTests()
    {
        _repoMock = new Mock<IBoardRepository>();
        _parserMock = new Mock<ITaskParserService>();
        _economyMock = new Mock<IGardenEconomyService>();
        _service = new BoardService(_repoMock.Object, _parserMock.Object, _economyMock.Object);
    }

    [Fact]
    public async Task CreateDefaultTaskEntity_ShouldReturnPopulatedTask()
    {
        var result = await _service.CreateDefaultTaskEntity("col-1");

        Assert.NotNull(result);
        Assert.Equal("col-1", result.ColumnId);
        Assert.Equal("New Task", result.Title);
        Assert.Equal(TaskPriority.Low, result.Priority);
        Assert.False(string.IsNullOrEmpty(result.Id));
    }

    [Fact]
    public async Task UpdateTaskFromUiAsync_ShouldParseStringsAndSave()
    {
        var entity = new TaskEntity { Id = "t1" };
        var title = "New Title";
        var priority = TaskPriority.High;
        var date = new DateTime(2026, 1, 1);

        await _service.UpdateTaskFromUiAsync(entity, title, "desc", "tag", priority, date);

        Assert.Equal(title, entity.Title);
        Assert.Equal(TaskPriority.High, entity.Priority);
        Assert.Equal(2026, entity.DueDate.Year);
        _repoMock.Verify(r => r.UpdateTaskAsync(entity), Times.Once);
    }

    [Fact]
    public async Task MoveTasksWithEconomyAsync_ToDone_ShouldTriggerReward()
    {
        var ids = new List<string> { "t1", "t2" };
        var colSource = new ColumnEntity { Id = "src", Tasks = new List<TaskEntity> { new() { Id = "t1" }, new() { Id = "t2" } } };
        var colTarget = new ColumnEntity { Id = "dest", Title = "Done", Tasks = new List<TaskEntity>() };
        
        _repoMock.Setup(r => r.LoadBoardAsync()).ReturnsAsync(new List<ColumnEntity> { colSource, colTarget });
        await _service.LoadBoardAsync();

        await _service.MoveTasksWithEconomyAsync(ids, "dest", "Done", 0);

        _economyMock.Verify(e => e.EarnWater(40), Times.Once);
    }

    [Fact]
    public async Task AddTaskAsync_ShouldUpdateRepoAndCache()
    {
        var columnId = "col-1";
        var task = new TaskEntity { Id = "task-1", Title = "Test Task" };
        var initialBoard = new List<ColumnEntity> { new() { Id = columnId, Tasks = new() } };
        
        _repoMock.Setup(r => r.LoadBoardAsync()).ReturnsAsync(initialBoard);
        await _service.LoadBoardAsync();

        await _service.AddTaskAsync(task, columnId);

        _repoMock.Verify(r => r.AddTaskAsync(task, columnId), Times.Once);
        Assert.Contains(task, initialBoard[0].Tasks);
    }

    [Fact]
    public async Task MoveTaskAsync_ShouldRelocateTaskInCache()
    {
        var task = new TaskEntity { Id = "t1", ColumnId = "c1" };
        var col1 = new ColumnEntity { Id = "c1", Tasks = new List<TaskEntity> { task } };
        var col2 = new ColumnEntity { Id = "c2", Tasks = new List<TaskEntity>() };
    
        _repoMock.Setup(r => r.LoadBoardAsync()).ReturnsAsync(new List<ColumnEntity> { col1, col2 });
        await _service.LoadBoardAsync();

        await _service.MoveTaskAsync(task, "c2", 0);

        Assert.Empty(col1.Tasks);
        Assert.Single(col2.Tasks);
        Assert.Equal("c2", col2.Tasks[0].ColumnId); 
    }

    [Fact]
    public async Task DeleteMultipleTasksAsync_ShouldRemoveFromAllColumns()
    {
        var ids = new List<string> { "1", "2" };
        var col = new ColumnEntity 
        { 
            Tasks = new List<TaskEntity> { new() { Id = "1" }, new() { Id = "2" }, new() { Id = "3" } } 
        };
        
        _repoMock.Setup(r => r.LoadBoardAsync()).ReturnsAsync(new List<ColumnEntity> { col });
        await _service.LoadBoardAsync();

        await _service.DeleteMultipleTasksAsync(ids);

        _repoMock.Verify(r => r.DeleteTasksAsync(ids), Times.Once);
        Assert.Single(col.Tasks);
        Assert.Equal("3", col.Tasks[0].Id);
    }

    [Fact]
    public async Task ProcessSmartPasteAsync_ShouldMatchColumnsCaseInsensitive()
    {
        var markdown = "some text";
        var task = new TaskEntity { Title = "Pasted Task" };
        var parsedResults = new List<ParsedTaskResult> { new ParsedTaskResult(task, "done") };
        var board = new List<ColumnEntity> { new() { Id = "id-done", Title = "Done", Tasks = new() } };

        _parserMock.Setup(p => p.ParseClipboardText(markdown)).Returns(parsedResults);
        _repoMock.Setup(r => r.LoadBoardAsync()).ReturnsAsync(board);

        var results = await _service.ProcessSmartPasteAsync(markdown);

        Assert.Single(results);
        _repoMock.Verify(r => r.AddTaskAsync(task, "id-done"), Times.Once);
    }

    [Fact]
    public async Task WipeBoardAsync_ShouldClearAllTasks()
    {
        var col = new ColumnEntity { Tasks = new List<TaskEntity> { new(), new() } };
        _repoMock.Setup(r => r.LoadBoardAsync()).ReturnsAsync(new List<ColumnEntity> { col });
        await _service.LoadBoardAsync();

        await _service.WipeBoardAsync();

        _repoMock.Verify(r => r.DeleteAllTasksAsync(), Times.Once);
        Assert.Empty(col.Tasks);
    }

    [Fact]
    public async Task MoveMultipleTasksAsync_ShouldHandleGroupMovement()
    {
        var ids = new List<string> { "t1", "t2" };
        var colSource = new ColumnEntity { Id = "src", Tasks = new List<TaskEntity> { new() { Id = "t1" }, new() { Id = "t2" } } };
        var colTarget = new ColumnEntity { Id = "dest", Tasks = new List<TaskEntity>() };
        
        _repoMock.Setup(r => r.LoadBoardAsync()).ReturnsAsync(new List<ColumnEntity> { colSource, colTarget });
        await _service.LoadBoardAsync();

        await _service.MoveMultipleTasksAsync(ids, "dest", 0);

        _repoMock.Verify(r => r.MoveMultipleTasksAsync(ids, "dest", 0), Times.Once);
        Assert.Equal(2, colTarget.Tasks.Count);
        Assert.Empty(colSource.Tasks);
    }
}