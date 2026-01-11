using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Terrarium.Core.Enums.Kanban;

namespace Terrarium.Avalonia.Helpers;

public class PriorityToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TaskPriority priority)
        {
            return priority switch
            {
                TaskPriority.High => Brushes.IndianRed,
                TaskPriority.Medium => Brushes.Khaki,
                TaskPriority.Low => Brushes.LightSteelBlue,
                _ => Brushes.Gray
            };
        }
        return Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) 
        => throw new NotImplementedException();
}