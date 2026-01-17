using Terrarium.Core.Models.Hierarchy;

namespace Terrarium.Core.Interfaces.Hierarchy;

public interface IProjectService
{
    Task<ProjectEntity> CreateProjectAsync(string workspaceId, string name);
    Task DeleteProjectAsync(string id);
}