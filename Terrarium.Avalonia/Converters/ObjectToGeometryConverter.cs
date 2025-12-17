using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Terrarium.Avalonia.Converters
{
    public class ObjectToGeometryConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // If the value is already a Geometry (like from our Resources), just pass it through
            if (value is Geometry geometry)
                return geometry;

            // If the value is a String (like "M10,10 L50,50"), parse it into a Geometry
            if (value is string pathData && !string.IsNullOrWhiteSpace(pathData))
                return StreamGeometry.Parse(pathData);

            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}