using Terrarium.Avalonia.ViewModels.Core;

namespace Terrarium.Avalonia.ViewModels;

public class WorkspaceViewModel : ViewModelBase
{
    public SidebarViewModel SidebarVm { get; }

    public WorkspaceViewModel(SidebarViewModel sidebarVm)
    {
        SidebarVm = sidebarVm;
    }
}