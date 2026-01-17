using Terrarium.Core.Interfaces.Hierarchy;
using Terrarium.Core.Interfaces.Repositories;
using Terrarium.Core.Models.Hierarchy;

namespace Terrarium.Logic.Services.Hierarchy;

public class WorkspaceService : IWorkspaceService
{
    private readonly IWorkspaceRepository _repository;

    public WorkspaceService(IWorkspaceRepository repository)
    {
        _repository = repository;
    }

    public async Task<WorkspaceEntity> CreateWorkspaceAsync(string orgId, string name)
    {
        var newWorkspace = new WorkspaceEntity
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            OrganizationId = orgId,
            LastModifiedUtc = DateTime.UtcNow,
            Projects = new List<ProjectEntity>()
        };

        await _repository.AddAsync(newWorkspace);
        return newWorkspace;
    }

    public async Task DeleteWorkspaceAsync(string id)
    {
        await _repository.DeleteAsync(id);
    }
}