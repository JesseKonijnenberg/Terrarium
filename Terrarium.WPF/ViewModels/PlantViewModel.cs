using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Terrarium.Core.Interfaces;
using Terrarium.Core.Models;
using Terrarium.Logic.Services;

namespace Terrarium.WPF.ViewModels
{
    public class PlantViewModel : INotifyPropertyChanged
    {
        private readonly IWeatherService _weatherService;
        private readonly PlantGrowthService _growthService;

        private Plant _myPlant;

        private readonly DispatcherTimer _gameTimer;
        public double Hydration => _myPlant.Hydration;
        public double Sunlight => _myPlant.Sunlight;

        private string _currentWeatherText = "Waiting for forecast...";
        public string CurrentWeatherText
        {
            get => _currentWeatherText;
            set { _currentWeatherText = value; OnPropertyChanged(); }
        }
        public string StatusMessage => $"{_myPlant.Name} is feeling {_myPlant.State}!";
        public ICommand WaterCommand { get; }

        public string PlantImage
        {
            get
            {
                return _myPlant.State switch
                {
                    PlantState.Thirsty => "/Images/plant_thirsty.png",
                    PlantState.Wilting => "/Images/plant_sad.png",
                    _ => "/Images/plant_happy.png"
                };
            }
        }

        public PlantViewModel(IWeatherService weatherService, PlantGrowthService growthService)
        {
            _weatherService = weatherService;
            _growthService = growthService;

            _myPlant = new Plant { Name = "Sprout", Hydration = 50, Sunlight = 50, State = PlantState.Happy };

            WaterCommand = new RelayCommand(WaterPlant);

            _gameTimer = new DispatcherTimer();
            _gameTimer.Interval = TimeSpan.FromSeconds(3);
            _gameTimer.Tick += GameLoop_Tick;
            _gameTimer.Start();
        }

        private async void GameLoop_Tick(object? sender, EventArgs e)
        {
            var weather = await _weatherService.GetCurrentWeatherAsync();
            _growthService.ApplyWeatherEffects(_myPlant, weather);
            CurrentWeatherText = weather.IsRaining ? "🌧️ Raining" : (weather.IsSunny ? "☀️ Sunny" : "☁️ Cloudy");

            OnPropertyChanged(nameof(Hydration));
            OnPropertyChanged(nameof(Sunlight));
            OnPropertyChanged(nameof(StatusMessage));
            OnPropertyChanged(nameof(PlantImage));

            System.Diagnostics.Debug.WriteLine($"TICK: Hydration={_myPlant.Hydration}, Sun={_myPlant.Sunlight}");
        }

        private void WaterPlant(object obj)
        {
            double targetHydration = _myPlant.Hydration + 30;
            if (targetHydration > 100)
            {
                targetHydration = 100;
            }
            _myPlant.Hydration = targetHydration;

            OnPropertyChanged(nameof(Hydration));
            OnPropertyChanged(nameof(StatusMessage));
            OnPropertyChanged(nameof(PlantImage));
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}