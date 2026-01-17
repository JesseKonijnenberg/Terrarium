using Terrarium.Core.Models.Hierarchy;

namespace Terrarium.Core.Interfaces.Hierarchy;

public interface IWorkspaceService
{
    Task<WorkspaceEntity> CreateWorkspaceAsync(string orgId, string name);
    Task DeleteWorkspaceAsync(string id);
}