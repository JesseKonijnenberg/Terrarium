using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Terrarium.Avalonia.Helpers;

public class IsTypeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null) return false;
        
        // Check if the current value (CurrentPage) is the type specified in the parameter
        var targetTypeParam = parameter as Type;
        return value.GetType() == targetTypeParam;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}