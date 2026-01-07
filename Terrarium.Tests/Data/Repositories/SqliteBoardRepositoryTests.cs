using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Terrarium.Core.Models.Kanban;
using Terrarium.Data;
using Terrarium.Data.Contexts;

namespace Terrarium.Tests.Data.Repositories;

public class SqliteBoardRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly TerrariumDbContext _context;
    private readonly SqliteBoardRepository _repo;

    public SqliteBoardRepositoryTests()
    {
        _connection = new SqliteConnection($"Data Source={Guid.NewGuid()};Mode=Memory;Cache=Shared");
        _connection.Open();

        var options = new DbContextOptionsBuilder<TerrariumDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new TerrariumDbContext(options);
        _context.Database.EnsureCreated();

        _repo = new SqliteBoardRepository(_context);
    }

    private async Task CreateColumnAsync(string id, string title = "Test Column")
    {
        _context.Columns.Add(new ColumnEntity { Id = id, Title = title });
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task AddTaskAsync_ShouldPersistTaskToDatabase()
    {
        var columnId = Guid.NewGuid().ToString();
        await CreateColumnAsync(columnId);

        var task = new TaskEntity { Id = "t1", Title = "T", Tag = "G", Description = "" };
        await _repo.AddTaskAsync(task, columnId);

        var savedTask = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == "t1");
        Assert.NotNull(savedTask);
    }

    [Fact]
    public async Task MoveMultipleTasksAsync_ShouldUpdateAllTasks()
    {
        var col1 = Guid.NewGuid().ToString();
        var col2 = Guid.NewGuid().ToString();
        await CreateColumnAsync(col1);
        await CreateColumnAsync(col2);

        var ids = new List<string> { "1", "2" };
        await _repo.AddTaskAsync(new TaskEntity { Id = "1", Title = "T1", Tag = "G", Description = "" }, col1);
        await _repo.AddTaskAsync(new TaskEntity { Id = "2", Title = "T2", Tag = "G", Description = "" }, col1);

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
        await _repo.AddTaskAsync(new TaskEntity { Id = "d1", Title = "T", Tag = "G", Description = "" }, colId);
        await _repo.AddTaskAsync(new TaskEntity { Id = "d2", Title = "T", Tag = "G", Description = "" }, colId);
        await _repo.AddTaskAsync(new TaskEntity { Id = "keep", Title = "T", Tag = "G", Description = "" }, colId);

        await _repo.DeleteTasksAsync(ids);

        var remainingCount = await _context.Tasks.CountAsync();
        Assert.Equal(1, remainingCount);
    }

    [Fact]
    public async Task UpdateTaskAsync_ShouldModifyExistingRecord()
    {
        var colId = Guid.NewGuid().ToString();
        await CreateColumnAsync(colId);

        var task = new TaskEntity { Id = "u1", Title = "Old", Tag = "G", Description = "" };
        await _repo.AddTaskAsync(task, colId);
        
        task.Title = "New";
        await _repo.UpdateTaskAsync(task);

        var updated = await _context.Tasks.FindAsync("u1");
        Assert.Equal("New", updated?.Title);
    }

    [Fact]
    public async Task LoadBoardAsync_ShouldReturnOrderedTasks()
    {
        var colId = Guid.NewGuid().ToString();
        await CreateColumnAsync(colId);

        // We use the Repo to add them so the Order logic is handled naturally
        await _repo.AddTaskAsync(new TaskEntity { Id = "task-last", Title = "Second", Tag = "G", Description = "" }, colId);
        await _repo.AddTaskAsync(new TaskEntity { Id = "task-first", Title = "First", Tag = "G", Description = "" }, colId);

        // Adjusting manual orders to verify sorting
        var tFirst = await _context.Tasks.FindAsync("task-first");
        var tLast = await _context.Tasks.FindAsync("task-last");
        tFirst!.Order = 0;
        tLast!.Order = 1;
        await _context.SaveChangesAsync();

        var board = await _repo.LoadBoardAsync();
        var resultTasks = board.First(c => c.Id == colId).Tasks;

        Assert.Equal("task-first", resultTasks[0].Id);
        Assert.Equal("task-last", resultTasks[1].Id);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Close();
        _connection.Dispose();
    }
}