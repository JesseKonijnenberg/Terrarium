namespace Terrarium.Core.Models
{
    public class Plant
    {
        private double _hydration;
        private double _sunlight;

        public required string Name { get; set; }
        public double Hydration
        {
            get => _hydration;
            set => _hydration = ValidatePercent(value);
        }
        public double Sunlight
        {
            get => _sunlight;
            set => _sunlight = ValidatePercent(value);
        }
        public PlantState State { get; set; }

        private static double ValidatePercent(double value)
        {
            if (value is < 0 or > 100)
                throw new ArgumentException("Value must be between 0 and 100.");

            return value;
        }
    }

    public enum PlantState{
        Happy,
        Thirsty,
        Wilting
    }
}
