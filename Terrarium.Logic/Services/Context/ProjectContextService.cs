using Terrarium.Core.Interfaces.Context;
using Terrarium.Core.Models.Context;

namespace Terrarium.Logic.Services.Context;

// In Terrarium.Logic
public class ProjectContextService : IProjectContextService
{
    public ProjectContext CurrentContext { get; private set; } = ProjectContext.Empty();
    
    public event Action<ProjectContext>? ContextChanged;

    public void UpdateContext(string? orgId, string? wsId, string? projId)
    {
        var newContext = new ProjectContext(orgId, wsId, projId);
        
        if (CurrentContext == newContext) return;

        CurrentContext = newContext;
        
        ContextChanged?.Invoke(CurrentContext);
    }
}