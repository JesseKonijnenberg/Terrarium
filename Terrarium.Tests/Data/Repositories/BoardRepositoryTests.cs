using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Terrarium.Core.Models.Kanban;
using Terrarium.Data.Contexts;
using Terrarium.Data.Repositories;

namespace Terrarium.Tests.Data.Repositories;

public class BoardRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly IDbContextFactory<TerrariumDbContext> _contextFactory;
    private readonly BoardRepository _repo;
    
    private const string TestProjectId = "default-project";
    private const string TestWorkspaceId = "solo-workspace";
    private const string TestBoardId = "main-board";

    public BoardRepositoryTests()
    {
        // Using a fresh, non-shared memory connection ensures 100% isolation per test.
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<TerrariumDbContext>()
            .UseSqlite(_connection)
            .Options;

        // Create the schema
        using (var context = new TerrariumDbContext(options))
        {
            context.Database.EnsureCreated();
        }

        // Mock the factory to return a new context connected to the SAME in-memory connection
        var mockFactory = new Mock<IDbContextFactory<TerrariumDbContext>>();
        mockFactory.Setup(f => f.CreateDbContext())
            .Returns(() => new TerrariumDbContext(options));
        mockFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new TerrariumDbContext(options));

        _contextFactory = mockFactory.Object;
        
        SeedRequiredData();
        _repo = new BoardRepository(_contextFactory);
    }

    private void SeedRequiredData()
    {
        using var context = _contextFactory.CreateDbContext();
        
        // We check for existence first to prevent UNIQUE constraint errors
        // if the DbContext already seeded these IDs during EnsureCreated().
        
        if (!context.KanbanBoards.Any(b => b.Id == TestBoardId))
        {
            context.KanbanBoards.Add(new KanbanBoardEntity 
            { 
                Id = TestBoardId, 
                ProjectId = TestProjectId, 
                Name = "Board",
                Project = null! // Prevent disconnected graph error
            });
        }
        
        if (!context.Columns.Any(c => c.Id == "col-1"))
        {
            context.Columns.Add(new ColumnEntity { Id = "col-1", Title = "Backlog", Order = 0, KanbanBoardId = TestBoardId });
        }
        
        if (!context.Columns.Any(c => c.Id == "col-2"))
        {
            context.Columns.Add(new ColumnEntity { Id = "col-2", Title = "Done", Order = 1, KanbanBoardId = TestBoardId });
        }
        
        context.SaveChanges();
    }

    [Fact]
    public async Task AddTask_WithRequiredMembers_ShouldPersistSuccessfully()
    {
        // ARRANGE
        var newTask = new TaskEntity 
        { 
            Id = "t1", 
            Title = "Test Task", 
            ColumnId = "col-1",      
            ProjectId = TestProjectId,
            Column = null! // Prevent disconnected graph error
        };

        // ACT
        await _repo.AddTaskAsync(newTask, "col-1");

        // ASSERT
        using var context = await _contextFactory.CreateDbContextAsync();
        var saved = await context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == "t1");
        Assert.NotNull(saved);
        Assert.Equal("col-1", saved.ColumnId);
    }

    [Fact]
    public async Task AddTask_WhenBoardIsLoadedAndTracked_ShouldNotThrowConcurrencyException()
    {
        // 1. Load the board (simulates UI state where entities are tracked)
        var board = await _repo.GetBoardAsync(TestWorkspaceId, TestProjectId);
        Assert.NotNull(board);

        // 2. Create new task with required members
        var newTask = new TaskEntity 
        { 
            Id = "new-ui-task", 
            Title = "Fresh Task", 
            ColumnId = "col-1", 
            ProjectId = TestProjectId,
            Column = null!
        };

        // 3. Perform Add. 
        // We expect this NOT to throw DbUpdateConcurrencyException.
        var exception = await Record.ExceptionAsync(() => _repo.AddTaskAsync(newTask, "col-1"));
        
        Assert.Null(exception);
    }

    [Fact]
    public async Task AddTask_WhenTasksAreSelected_ShouldNotInterfereWithInsert()
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            var existingTask = new TaskEntity 
            { 
                Id = "selected-1", 
                Title = "Selected Task", 
                ProjectId = TestProjectId, 
                ColumnId = "col-1",
                Column = null!
            };
            context.Tasks.Add(existingTask);
            await context.SaveChangesAsync();
        }
        
        // Simulate "Selection" in a separate context (like the UI would have)
        // In the new factory pattern, the repo creates its own context, so this test is less relevant for concurrency
        // but still good to ensure no side effects.
        
        var newTask = new TaskEntity 
        { 
            Id = "added-during-selection", 
            Title = "New Task", 
            ColumnId = "col-1", 
            ProjectId = TestProjectId,
            Column = null!
        };
    
        // ACT
        await _repo.AddTaskAsync(newTask, "col-1");

        // ASSERT
        using var verifyContext = await _contextFactory.CreateDbContextAsync();
        Assert.True(await verifyContext.Tasks.AnyAsync(t => t.Id == "added-during-selection"));
    }

    [Fact]
    public async Task MoveMultipleTasks_ShouldUpdateSuccessfully()
    {
        // ARRANGE
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            var t1 = new TaskEntity { Id = "m1", Title = "T1", ColumnId = "col-1", ProjectId = TestProjectId, Column = null! };
            var t2 = new TaskEntity { Id = "m2", Title = "T2", ColumnId = "col-1", ProjectId = TestProjectId, Column = null! };
            context.Tasks.AddRange(t1, t2);
            await context.SaveChangesAsync();
        }

        // ACT
        await _repo.MoveMultipleTasksAsync(new List<string> { "m1", "m2" }, "col-2", 0);

        // ASSERT
        using var verifyContext = await _contextFactory.CreateDbContextAsync();
        var results = await verifyContext.Tasks.AsNoTracking().Where(t => t.ColumnId == "col-2").ToListAsync();
        Assert.Equal(2, results.Count);
        Assert.All(results, t => Assert.Equal("col-2", t.ColumnId));
    }

    [Fact]
    public async Task UpdateTask_ShouldNotThrowConcurrencyException_WhenTracked()
    {
        // ARRANGE
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            var task = new TaskEntity { Id = "u1", Title = "Old", ColumnId = "col-1", ProjectId = TestProjectId, Column = null! };
            context.Tasks.Add(task);
            await context.SaveChangesAsync();
        }
        
        // ACT
        var updated = new TaskEntity { Id = "u1", Title = "New", ColumnId = "col-1", ProjectId = TestProjectId, Column = null! };
        await _repo.UpdateTaskAsync(updated);

        // ASSERT
        using var verifyContext = await _contextFactory.CreateDbContextAsync();
        var result = await verifyContext.Tasks.AsNoTracking().FirstAsync(t => t.Id == "u1");
        Assert.Equal("New", result.Title);
    }

    public void Dispose()
    {
        // Proper cleanup of the in-memory database
        _connection.Close();
        _connection.Dispose();
    }
}