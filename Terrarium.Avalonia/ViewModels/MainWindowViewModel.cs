using System.Windows.Input;
using Terrarium.Avalonia.ViewModels.Core;

namespace Terrarium.Avalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public KanbanBoardViewModel BoardVm { get; }
        public GardenViewModel GardenVm { get; }
        public SettingsViewModel SettingsVm { get; }

        public ViewModelBase CurrentPage 
        { 
            get; 
            set 
            { 
                if (field == value) return; 
                field = value; 
                OnPropertyChanged(); 
            } 
        }

        public ICommand GoToBoardCommand { get; }
        public ICommand GoToGardenCommand { get; }
        public ICommand GoToSettingsCommand { get; }

        public MainWindowViewModel(
            KanbanBoardViewModel boardVm,
            GardenViewModel gardenVm,
            SettingsViewModel settingsVm)
        {
            BoardVm = boardVm;
            GardenVm = gardenVm;
            SettingsVm = settingsVm;
            CurrentPage = BoardVm;

            GoToBoardCommand = new RelayCommand(_ => CurrentPage = BoardVm);
            GoToGardenCommand = new RelayCommand(_ => CurrentPage = GardenVm);
            GoToSettingsCommand = new RelayCommand(_ => CurrentPage = SettingsVm);
        }
    }
}