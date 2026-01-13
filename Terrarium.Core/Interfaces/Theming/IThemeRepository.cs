using Terrarium.Core.Models.Theming;

namespace Terrarium.Core.Interfaces.Theming;

public interface IThemeRepository
{
    /// <summary>
    /// Retrieves all themes, including hardcoded ones and those found on disk.
    /// </summary>
    IEnumerable<ITheme> GetAllThemes();

    /// <summary>
    /// Finds a specific theme by its unique ID.
    /// </summary>
    ITheme? GetThemeById(string id);

    /// <summary>
    /// Refreshes the internal collection (e.g., if a new JSON file was added).
    /// </summary>
    void ReloadThemes();
}