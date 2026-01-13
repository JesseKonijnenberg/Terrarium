using Terrarium.Core.Interfaces.Theming;
using Terrarium.Core.Models.Hierarchy;
using Terrarium.Core.Models.Theming;

// For IThemeRepository

namespace Terrarium.Logic.Services.Theming;

public class ThemeService : IThemeService
{
    private readonly IThemeRepository _repository;

    public ThemeService(IThemeRepository repository)
    {
        _repository = repository;
    }

    public IEnumerable<ITheme> GetAvailableThemes() => _repository.GetAllThemes();

    public ITheme GetThemeForOrganization(OrganizationEntity org)
    {
        var theme = _repository.GetThemeById(org.ActiveThemeId);
        
        // Safety fallback: if a custom theme was deleted from disk, 
        // we revert to the built-in Sage Dark theme.
        return theme ?? _repository.GetThemeById("sage-dark")!;
    }

    public bool TrySetOrganizationTheme(OrganizationEntity org, ITheme theme)
    {
        if (org.LockTheme) return false;

        org.ActiveThemeId = theme.Id;
        return true;
    }
}