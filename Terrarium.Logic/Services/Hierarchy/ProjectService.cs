using Terrarium.Core.Interfaces.Hierarchy;
using Terrarium.Core.Interfaces.Kanban;
using Terrarium.Core.Interfaces.Repositories;
using Terrarium.Core.Models.Hierarchy;

namespace Terrarium.Logic.Services.Hierarchy;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _repository;
    private readonly IBoardService _boardService;

    public ProjectService(IProjectRepository repository, IBoardService boardService)
    {
        _repository = repository;
        _boardService = boardService;
    }

    public async Task<ProjectEntity> CreateProjectAsync(string workspaceId, string name)
    {
        var projectId = Guid.NewGuid().ToString();
        
        var newProject = new ProjectEntity
        {
            Id = projectId,
            Name = name,
            WorkspaceId = workspaceId,
            LastModifiedUtc = DateTime.UtcNow
        };
        
        await _repository.AddAsync(newProject);

        // 2. Automatically initialize the default Kanban Board for this project
        // This ensures the board is ready as soon as the user clicks it.
        // (Assuming you add a CreateBoardAsync to IBoardService, see note below)
        //await _boardService.CreateBoardAsync(workspaceId, projectId);

        return newProject;
    }

    public async Task DeleteProjectAsync(string id)
    {
        await _repository.DeleteAsync(id);
    }
}