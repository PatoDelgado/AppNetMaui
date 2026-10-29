using System.Globalization;
using HolaMundo.Models;

namespace HolaMundo.Converters;

public class EstadoAColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool oscuro = Application.Current?.RequestedTheme == AppTheme.Dark;

        if (value is not EstadoEntrega estado)
            return Colors.Gray;

        return estado switch
        {
            EstadoEntrega.Entregada => oscuro ? Color.FromArgb("#66BB6A") : Color.FromArgb("#2E7D32"),
            EstadoEntrega.Fallida   => oscuro ? Color.FromArgb("#EF5350") : Color.FromArgb("#C62828"),
            _                       => oscuro ? Color.FromArgb("#FFB74D") : Color.FromArgb("#FF8811"),
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
