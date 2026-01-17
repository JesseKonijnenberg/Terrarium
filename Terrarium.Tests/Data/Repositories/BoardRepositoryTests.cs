using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Terrarium.Core.Models.Kanban;
using Terrarium.Data.Contexts;
using Terrarium.Data.Repositories;

namespace Terrarium.Tests.Data.Repositories;

public class BoardRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly TerrariumDbContext _context;
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

        _context = new TerrariumDbContext(options);
        
        // This creates the schema and applies any HasData seeds from OnModelCreating
        _context.Database.EnsureCreated();
        
        SeedRequiredData();
        _repo = new BoardRepository(_context);
    }

    private void SeedRequiredData()
    {
        // We check for existence first to prevent UNIQUE constraint errors
        // if the DbContext already seeded these IDs during EnsureCreated().
        
        if (!_context.KanbanBoards.Any(b => b.Id == TestBoardId))
        {
            _context.KanbanBoards.Add(new KanbanBoardEntity 
            { 
                Id = TestBoardId, 
                ProjectId = TestProjectId, 
                Name = "Board" 
            });
        }
        
        if (!_context.Columns.Any(c => c.Id == "col-1"))
        {
            _context.Columns.Add(new ColumnEntity { Id = "col-1", Title = "Backlog", Order = 0, KanbanBoardId = TestBoardId });
        }
        
        if (!_context.Columns.Any(c => c.Id == "col-2"))
        {
            _context.Columns.Add(new ColumnEntity { Id = "col-2", Title = "Done", Order = 1, KanbanBoardId = TestBoardId });
        }
        
        _context.SaveChanges();
        
        // Detach everything so the repository has to actually fetch data from the DB,
        // simulating a real application restart/navigation.
        _context.ChangeTracker.Clear();
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
            ProjectId = TestProjectId 
        };

        // ACT
        await _repo.AddTaskAsync(newTask, "col-1");

        // ASSERT
        var saved = await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == "t1");
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
            ProjectId = TestProjectId 
        };

        // 3. Perform Add. 
        // We expect this NOT to throw DbUpdateConcurrencyException.
        var exception = await Record.ExceptionAsync(() => _repo.AddTaskAsync(newTask, "col-1"));
        
        Assert.Null(exception);
    }

    [Fact]
    public async Task AddTask_WhenTasksAreSelected_ShouldNotInterfereWithInsert()
    {
        var existingTask = new TaskEntity 
        { 
            Id = "selected-1", 
            Title = "Selected Task", 
            ProjectId = TestProjectId, 
            ColumnId = "col-1" 
        };
        _context.Tasks.Add(existingTask);
        await _context.SaveChangesAsync();
        
        _context.ChangeTracker.Clear(); 
        
        var board = await _repo.GetBoardAsync(TestWorkspaceId, TestProjectId);
        var taskToSelect = board!.Columns.First().Tasks.First();
        
        _context.Entry(taskToSelect).State = EntityState.Modified; 
        
        var newTask = new TaskEntity 
        { 
            Id = "added-during-selection", 
            Title = "New Task", 
            ColumnId = "col-1", 
            ProjectId = TestProjectId 
        };
    
        // ACT
        await _repo.AddTaskAsync(newTask, "col-1");

        // ASSERT
        Assert.True(await _context.Tasks.AnyAsync(t => t.Id == "added-during-selection"));
    }

    [Fact]
    public async Task MoveMultipleTasks_ShouldUpdateSuccessfully()
    {
        // ARRANGE
        var t1 = new TaskEntity { Id = "m1", Title = "T1", ColumnId = "col-1", ProjectId = TestProjectId };
        var t2 = new TaskEntity { Id = "m2", Title = "T2", ColumnId = "col-1", ProjectId = TestProjectId };
        _context.Tasks.AddRange(t1, t2);
        await _context.SaveChangesAsync();

        // ACT
        await _repo.MoveMultipleTasksAsync(new List<string> { "m1", "m2" }, "col-2", 0);

        // ASSERT
        var results = await _context.Tasks.AsNoTracking().Where(t => t.ColumnId == "col-2").ToListAsync();
        Assert.Equal(2, results.Count);
        Assert.All(results, t => Assert.Equal("col-2", t.ColumnId));
    }

    [Fact]
    public async Task UpdateTask_ShouldNotThrowConcurrencyException_WhenTracked()
    {
        // ARRANGE
        var task = new TaskEntity { Id = "u1", Title = "Old", ColumnId = "col-1", ProjectId = TestProjectId };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        
        // Load it into the tracker
        var loaded = await _context.Tasks.FindAsync("u1");
        
        // ACT
        var updated = new TaskEntity { Id = "u1", Title = "New", ColumnId = "col-1", ProjectId = TestProjectId };
        await _repo.UpdateTaskAsync(updated);

        // ASSERT
        var result = await _context.Tasks.AsNoTracking().FirstAsync(t => t.Id == "u1");
        Assert.Equal("New", result.Title);
    }

    public void Dispose()
    {
        // Proper cleanup of the in-memory database
        _context.Dispose();
        _connection.Close();
        _connection.Dispose();
    }
}