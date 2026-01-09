using System.Reflection;

namespace Terrarium.Avalonia.Helpers;

public static class AppVersion
{
    public static string Get()
    {
        var version = Assembly.GetEntryAssembly()?.GetName().Version;
        return version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "Dev";
    }
}