using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Terrarium.Core.Interfaces;
using Terrarium.Core.Models;
using Terrarium.Logic.Services;

namespace Terrarium.Avalonia.ViewModels
{
    public partial class PlantViewModel : ObservableObject
    {
        private readonly IWeatherService _weatherService;
        private readonly PlantGrowthService _growthService;
        private readonly Plant _myPlant;
        private readonly DispatcherTimer _gameTimer;

        public double Hydration => _myPlant.Hydration;
        public double Sunlight => _myPlant.Sunlight;

        [ObservableProperty]
        private string _currentWeatherText = "Waiting for forecast...";

        public string StatusMessage => $"{_myPlant.Name} is feeling {_myPlant.State}!";

        [ObservableProperty]
        private Bitmap? _plantImage;

        public ICommand WaterCommand { get; }

        public PlantViewModel(IWeatherService weatherService, PlantGrowthService growthService)
        {
            _weatherService = weatherService;
            _growthService = growthService;

            _myPlant = new Plant { Name = "Liesje (v1.0.8 extra speciale versie)", Hydration = 50, Sunlight = 50, State = PlantState.Happy };

            WaterCommand = new RelayCommand(() => WaterPlant());

            _gameTimer = new DispatcherTimer();
            _gameTimer.Interval = TimeSpan.FromSeconds(3);
            _gameTimer.Tick += GameLoop_Tick;
            _gameTimer.Start();

            GameLoop_Tick(this, EventArgs.Empty);
        }

        private async void GameLoop_Tick(object? sender, EventArgs e)
        {
            var weather = await _weatherService.GetCurrentWeatherAsync();

            _growthService.ApplyWeatherEffects(_myPlant, weather);

            CurrentWeatherText = weather.IsRaining ? "🌧️ Raining" : (weather.IsSunny ? "☀️ Sunny" : "☁️ Cloudy");

            OnPropertyChanged(nameof(Hydration));
            OnPropertyChanged(nameof(Sunlight));
            OnPropertyChanged(nameof(StatusMessage));

            UpdatePlantImage();
        }

        private void WaterPlant()
        {
            double targetHydration = _myPlant.Hydration + 30;

            if (targetHydration > 100) targetHydration = 100;

            _myPlant.Hydration = targetHydration;

            OnPropertyChanged(nameof(Hydration));
            OnPropertyChanged(nameof(StatusMessage));
            UpdatePlantImage();
        }

        private void UpdatePlantImage()
        {
            string fileName = _myPlant.State switch
            {
                PlantState.Thirsty => "plant_thirsty.png",
                PlantState.Wilting => "plant_sad.png",
                _ => "plant_happy.png"
            };
            PlantImage = LoadBitmap(fileName);
        }

        private Bitmap? LoadBitmap(string fileName)
        {
            var uri = new Uri($"avares://Terrarium.Avalonia/Images/{fileName}");

            try
            {
                return new Bitmap(AssetLoader.Open(uri));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading image: {ex.Message}");
                return null;
            }
        }
    }
}
