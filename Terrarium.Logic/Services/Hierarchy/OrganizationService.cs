using Terrarium.Core.Interfaces.Hierarchy;
using Terrarium.Core.Interfaces.Repositories;
using Terrarium.Core.Models.Hierarchy;

namespace Terrarium.Logic.Services.Hierarchy;

public class OrganizationService : IOrganizationService
{
    private readonly IOrganizationRepository _repository;

    public OrganizationService(IOrganizationRepository repository)
    {
        _repository = repository;
    }

    public async Task<OrganizationEntity> CreateOrganizationAsync(string name)
    {
        var newOrg = new OrganizationEntity
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            LastModifiedUtc = DateTime.UtcNow,
            Workspaces = new List<WorkspaceEntity>()
        };

        await _repository.AddAsync(newOrg);
        return newOrg;
    }

    public async Task DeleteOrganizationAsync(string id)
    {
        await _repository.DeleteAsync(id);
    }
}