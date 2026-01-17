using Terrarium.Core.Models.Hierarchy;

namespace Terrarium.Core.Interfaces.Repositories;

public interface IOrganizationRepository
{
    Task<OrganizationEntity?> GetByIdAsync(string id);
    Task AddAsync(OrganizationEntity entity);
    Task DeleteAsync(string id);
}