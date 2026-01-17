using Terrarium.Core.Models.Hierarchy;

namespace Terrarium.Core.Interfaces.Hierarchy;

public interface IOrganizationService
{
    Task<OrganizationEntity> CreateOrganizationAsync(string name);
    Task DeleteOrganizationAsync(string id);
}