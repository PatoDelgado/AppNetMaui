using System.Globalization;
using HolaMundo.Models;

namespace HolaMundo.Converters;

public class EstadoADisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not EstadoEntrega estado)
            return "⏳ Pendiente";

        return estado switch
        {
            EstadoEntrega.Entregada => "✅ Entregada",
            EstadoEntrega.Fallida   => "❌ Fallida",
            _                       => "⏳ Pendiente",
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
