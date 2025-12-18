using Terrarium.Avalonia.ViewModels.Core;


namespace Terrarium.Avalonia.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        public string AppVersion => $"v{Helpers.AppVersion.Get()}";

        public SettingsViewModel()
        {

        }
    }
}