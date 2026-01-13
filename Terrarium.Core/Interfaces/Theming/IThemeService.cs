using Terrarium.Core.Models.Hierarchy;
using Terrarium.Core.Models.Theming;

namespace Terrarium.Core.Interfaces.Theming;

public interface IThemeService
{
    /// <summary>
    /// Gets all themes currently available in the system.
    /// </summary>
    IEnumerable<ITheme> GetAvailableThemes();

    /// <summary>
    /// Resolves the theme for a specific organization.
    /// </summary>
    ITheme GetThemeForOrganization(OrganizationEntity org);

    /// <summary>
    /// Updates the theme for an organization if it isn't locked.
    /// </summary>
    bool TrySetOrganizationTheme(OrganizationEntity org, ITheme theme);
}