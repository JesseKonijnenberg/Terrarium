using System.Windows.Input;
using Terrarium.Avalonia.ViewModels.Core;

namespace Terrarium.Avalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public KanbanBoardViewModel BoardVm { get; } = new KanbanBoardViewModel();
        public GardenViewModel GardenVm { get; } = new GardenViewModel();

        private ViewModelBase _currentPage;
        public ViewModelBase CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand GoToBoardCommand { get; }
        public ICommand GoToGardenCommand { get; }

        public MainWindowViewModel()
        {
            _currentPage = BoardVm;

            GoToBoardCommand = new RelayCommand(_ => CurrentPage = BoardVm);
            GoToGardenCommand = new RelayCommand(_ => CurrentPage = GardenVm);
        }
    }
}