using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Terrarium.Core.Models.Hierarchy;
using Terrarium.Core.Models.Kanban;
using Terrarium.Data.Contexts;
using Terrarium.Data.Repositories;

namespace Terrarium.Tests.Data.Repositories;

public class SqliteBoardRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly TerrariumDbContext _context;
    private readonly SqliteBoardRepository _repo;
    private const string TestWorkspaceId = "test-workspace";

    public SqliteBoardRepositoryTests()
    {
        _connection = new SqliteConnection($"Data Source={Guid.NewGuid()};Mode=Memory;Cache=Shared");
        _connection.Open();

        var options = new DbContextOptionsBuilder<TerrariumDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new TerrariumDbContext(options);
        _context.Database.EnsureCreated();
        
        _context.Workspaces.Add(new WorkspaceEntity 
        { 
            Id = TestWorkspaceId, 
            Name = "Test Workspace",
            IsPersonal = true 
        });
        _context.SaveChanges();

        _repo = new SqliteBoardRepository(_context);
    }

    private async Task CreateColumnAsync(string id, string title = "Test Column", string workspaceId = TestWorkspaceId)
    {
        _context.Columns.Add(new ColumnEntity { Id = id, Title = title, WorkspaceId = workspaceId });
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task AddTaskAsync_ShouldPersistTaskToDatabase()
    {
        var columnId = Guid.NewGuid().ToString();
        await CreateColumnAsync(columnId);

        var task = new TaskEntity { Id = "t1", Title = "T", Tag = "G", Description = "", WorkspaceId = TestWorkspaceId };
        await _repo.AddTaskAsync(task, columnId);

        var savedTask = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == "t1");
        Assert.NotNull(savedTask);
        Assert.Equal(TestWorkspaceId, savedTask.WorkspaceId);
    }

    [Fact]
    public async Task MoveMultipleTasksAsync_ShouldUpdateAllTasks()
    {
        var col1 = Guid.NewGuid().ToString();
        var col2 = Guid.NewGuid().ToString();
        await CreateColumnAsync(col1);
        await CreateColumnAsync(col2);

        var ids = new List<string> { "1", "2" };
        await _repo.AddTaskAsync(new TaskEntity { Id = "1", Title = "T1", Tag = "G", Description = "", WorkspaceId = TestWorkspaceId }, col1);
        await _repo.AddTaskAsync(new TaskEntity { Id = "2", Title = "T2", Tag = "G", Description = "", WorkspaceId = TestWorkspaceId }, col1);

        await _repo.MoveMultipleTasksAsync(ids, col2, 10);

        var tasks = await _context.Tasks.Where(t => t.ColumnId == col2).OrderBy(t => t.Order).ToListAsync();
        Assert.Equal(2, tasks.Count);
        Assert.Equal(10, tasks[0].Order);
    }

    [Fact]
    public async Task DeleteTasksAsync_ShouldBatchDeleteFromDatabase()
    {
        var colId = Guid.NewGuid().ToString();
        await CreateColumnAsync(colId);

        var ids = new List<string> { "d1", "d2" };
        await _repo.AddTaskAsync(new TaskEntity { Id = "d1", Title = "T", Tag = "G", Description = "", WorkspaceId = TestWorkspaceId }, colId);
        await _repo.AddTaskAsync(new TaskEntity { Id = "d2", Title = "T", Tag = "G", Description = "", WorkspaceId = TestWorkspaceId }, colId);
        await _repo.AddTaskAsync(new TaskEntity { Id = "keep", Title = "T", Tag = "G", Description = "", WorkspaceId = TestWorkspaceId }, colId);

        await _repo.DeleteTasksAsync(ids);

        var remainingCount = await _context.Tasks.CountAsync();
        Assert.Equal(1, remainingCount);
    }

    [Fact]
    public async Task UpdateTaskAsync_ShouldModifyExistingRecord()
    {
        var colId = Guid.NewGuid().ToString();
        await CreateColumnAsync(colId);

        var task = new TaskEntity { Id = "u1", Title = "Old", Tag = "G", Description = "", WorkspaceId = TestWorkspaceId };
        await _repo.AddTaskAsync(task, colId);
        
        task.Title = "New";
        await _repo.UpdateTaskAsync(task);

        var updated = await _context.Tasks.FindAsync("u1");
        Assert.Equal("New", updated?.Title);
    }

    [Fact]
    public async Task LoadBoardAsync_ShouldReturnOrderedTasksForWorkspace()
    {
        var colId = Guid.NewGuid().ToString();
        await CreateColumnAsync(colId);

        await _repo.AddTaskAsync(new TaskEntity { Id = "task-last", Title = "Second", Tag = "G", Description = "", WorkspaceId = TestWorkspaceId }, colId);
        await _repo.AddTaskAsync(new TaskEntity { Id = "task-first", Title = "First", Tag = "G", Description = "", WorkspaceId = TestWorkspaceId }, colId);

        var tFirst = await _context.Tasks.FindAsync("task-first");
        var tLast = await _context.Tasks.FindAsync("task-last");
        tFirst!.Order = 0;
        tLast!.Order = 1;
        await _context.SaveChangesAsync();

        var board = await _repo.LoadBoardAsync(TestWorkspaceId);
        var resultTasks = board.First(c => c.Id == colId).Tasks;

        Assert.Equal("task-first", resultTasks[0].Id);
        Assert.Equal("task-last", resultTasks[1].Id);
    }

    [Fact]
    public async Task AddTaskAsync_AfterLoadBoard_ShouldNotThrowConcurrencyException()
    {
        var colId = Guid.NewGuid().ToString();
        await CreateColumnAsync(colId);

        var board = await _repo.LoadBoardAsync(TestWorkspaceId);
        Assert.NotEmpty(board);

        var newTask = new TaskEntity 
        { 
            Id = "new-task", 
            Title = "New", 
            Tag = "G", 
            Description = "", 
            WorkspaceId = TestWorkspaceId 
        };
        
        await _repo.AddTaskAsync(newTask, colId);

        var saved = await _context.Tasks.FindAsync("new-task");
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task DeleteTasksAsync_AfterLoadBoard_ShouldNotThrowConcurrencyException()
    {
        var colId = Guid.NewGuid().ToString();
        await CreateColumnAsync(colId);

        var ids = new List<string> { "task1", "task2" };
        await _repo.AddTaskAsync(new TaskEntity { Id = "task1", Title = "T1", Tag = "G", Description = "", WorkspaceId = TestWorkspaceId }, colId);
        await _repo.AddTaskAsync(new TaskEntity { Id = "task2", Title = "T2", Tag = "G", Description = "", WorkspaceId = TestWorkspaceId }, colId);

        var board = await _repo.LoadBoardAsync(TestWorkspaceId);
        Assert.Equal(2, board.First().Tasks.Count);

        await _repo.DeleteTasksAsync(ids);

        var remaining = await _context.Tasks.CountAsync();
        Assert.Equal(0, remaining);
    }

    [Fact]
    public async Task DeleteTasksAsync_ShouldNotRequireSaveChanges()
    {
        var colId = Guid.NewGuid().ToString();
        await CreateColumnAsync(colId);

        await _repo.AddTaskAsync(new TaskEntity { Id = "delete-me", Title = "T", Tag = "G", Description = "", WorkspaceId = TestWorkspaceId }, colId);

        await _repo.DeleteTasksAsync(new[] { "delete-me" });

        var newContextOptions = new DbContextOptionsBuilder<TerrariumDbContext>()
            .UseSqlite(_connection)
            .Options;
        using var newContext = new TerrariumDbContext(newContextOptions);

        var count = await newContext.Tasks.CountAsync();
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task LoadBoard_ThenAdd_ThenLoadAgain_ShouldWorkWithoutConflict()
    {
        var colId = Guid.NewGuid().ToString();
        await CreateColumnAsync(colId);

        var board1 = await _repo.LoadBoardAsync(TestWorkspaceId);
        Assert.Empty(board1.First().Tasks);

        await _repo.AddTaskAsync(
            new TaskEntity { Id = "new", Title = "T", Tag = "G", Description = "", WorkspaceId = TestWorkspaceId }, 
            colId
        );

        var board2 = await _repo.LoadBoardAsync(TestWorkspaceId);
        Assert.Single(board2.First().Tasks);
    }

    [Fact]
    public async Task MultipleSequentialOperations_ShouldNotCauseTrackingConflicts()
    {
        var colId = Guid.NewGuid().ToString();
        await CreateColumnAsync(colId);

        var board = await _repo.LoadBoardAsync(TestWorkspaceId);

        await _repo.AddTaskAsync(
            new TaskEntity { Id = "task1", Title = "T1", Tag = "G", Description = "", WorkspaceId = TestWorkspaceId }, 
            colId
        );

        board = await _repo.LoadBoardAsync(TestWorkspaceId);

        var task = new TaskEntity { Id = "task1", Title = "Updated", Tag = "G", Description = "", WorkspaceId = TestWorkspaceId };
        await _repo.UpdateTaskAsync(task);

        board = await _repo.LoadBoardAsync(TestWorkspaceId);

        await _repo.DeleteTaskAsync("task1");

        var finalCount = await _context.Tasks.CountAsync();
        Assert.Equal(0, finalCount);
    }

    [Fact]
    public async Task LoadBoardAsync_ShouldNotTrackEntities()
    {
        var colId = Guid.NewGuid().ToString();
        await CreateColumnAsync(colId);
        await _repo.AddTaskAsync(
            new TaskEntity { Id = "track-test", Title = "T", Tag = "G", Description = "", WorkspaceId = TestWorkspaceId }, 
            colId
        );

        var board = await _repo.LoadBoardAsync(TestWorkspaceId);
        var loadedTask = board.First().Tasks.First();

        var entry = _context.Entry(loadedTask);
        Assert.Equal(EntityState.Detached, entry.State);
    }

    [Fact]
    public async Task DeleteMultipleTasks_ThenAddNew_ShouldWork()
    {
        var colId = Guid.NewGuid().ToString();
        await CreateColumnAsync(colId);
        
        await _repo.AddTaskAsync(new TaskEntity { Id = "old1", Title = "T1", Tag = "G", Description = "", WorkspaceId = TestWorkspaceId }, colId);
        await _repo.AddTaskAsync(new TaskEntity { Id = "old2", Title = "T2", Tag = "G", Description = "", WorkspaceId = TestWorkspaceId }, colId);
        
        await _repo.LoadBoardAsync(TestWorkspaceId);
        
        await _repo.DeleteTasksAsync(new[] { "old1", "old2" });
        
        await _repo.AddTaskAsync(
            new TaskEntity { Id = "new", Title = "New", Tag = "G", Description = "", WorkspaceId = TestWorkspaceId }, 
            colId
        );

        var tasks = await _context.Tasks.ToListAsync();
        Assert.Single(tasks);
        Assert.Equal("new", tasks[0].Id);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Close();
        _connection.Dispose();
    }
}