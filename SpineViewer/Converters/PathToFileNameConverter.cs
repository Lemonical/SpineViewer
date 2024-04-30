using Avalonia.Data.Converters;
using Avalonia.Data;
using System;
using System.Globalization;
using System.IO;

namespace SpineViewer.Converters;

public class PathToFileNameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string sourceText && targetType.IsAssignableTo(typeof(string)))
            return Path.GetFileName(sourceText);
        else if (value == null)
            return null;

        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}