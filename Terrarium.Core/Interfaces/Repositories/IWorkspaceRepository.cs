using Terrarium.Core.Models.Hierarchy;

namespace Terrarium.Core.Interfaces.Repositories;

public interface IWorkspaceRepository
{
    Task<WorkspaceEntity?> GetByIdAsync(string id);
    Task AddAsync(WorkspaceEntity entity);
    Task DeleteAsync(string id);
}