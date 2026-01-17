using Terrarium.Core.Models.Hierarchy;

namespace Terrarium.Core.Interfaces.Repositories;

public interface IProjectRepository
{
    Task<ProjectEntity?> GetByIdAsync(string id);
    Task AddAsync(ProjectEntity entity);
    Task DeleteAsync(string id);
}