using Terrarium.Core.Interfaces.Context;
using Terrarium.Core.Models.Context;

namespace Terrarium.Logic.Services.Context;

public class ProjectContextService : IProjectContextService
{
    public ProjectContext? CurrentContext { get; private set; }
    public event Action<ProjectContext>? ContextChanged;

    public void UpdateContext(string orgId, string workspaceId, string projectId)
    {
        // Prevent unnecessary updates if the context hasn't actually changed
        var newContext = new ProjectContext(orgId, workspaceId, projectId);
        if (newContext == CurrentContext) return;

        CurrentContext = newContext;
        ContextChanged?.Invoke(CurrentContext);
    }
}