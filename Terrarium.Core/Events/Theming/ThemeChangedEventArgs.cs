using Terrarium.Core.Models.Theming;

namespace Terrarium.Core.Events.Theming;

public class ThemeChangedEventArgs : EventArgs
{
    public ITheme NewTheme { get; }
    
    public ThemeChangedEventArgs(ITheme newTheme)
    {
        NewTheme = newTheme;
    }
}