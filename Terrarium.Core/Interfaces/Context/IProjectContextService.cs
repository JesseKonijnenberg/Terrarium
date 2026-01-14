using Terrarium.Core.Models.Context;

namespace Terrarium.Core.Interfaces.Context;

public interface IProjectContextService
{
    // Allows new plugins to see the current state immediately on creation
    ProjectContext? CurrentContext { get; }
    
    // Notifies active plugins when the user switches projects
    event Action<ProjectContext>? ContextChanged;

    void UpdateContext(string orgId, string workspaceId, string projectId);
}