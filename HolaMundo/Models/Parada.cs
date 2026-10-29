using SQLite;

namespace HolaMundo.Models;

public class Parada
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int Numero { get; set; }
    public int RepartidorId { get; set; }
    public string Cliente { get; set; } = "";
    public string Direccion { get; set; } = "";
    public EstadoEntrega Estado { get; set; } = EstadoEntrega.Pendiente;
    public string? RazonFallo { get; set; }
    public DateTime ModificadoEn { get; set; } = DateTime.Now;

    public double? Latitud { get; set; }
    public double? Longitud { get; set; }

    [Ignore]
    public string DistanciaTexto { get; set; } = "";

    [Ignore]
    public double DistanciaKm { get; set; } = double.MaxValue;

    [Ignore]
    public string TiempoEnFalloTexto
    {
        get
        {
            if (Estado != EstadoEntrega.Fallida) return string.Empty;
            var tiempo = DateTime.Now - ModificadoEn;
            if (tiempo.TotalMinutes < 1) return "Falló hace un momento";
            if (tiempo.TotalMinutes < 60) return $"Falló hace {(int)tiempo.TotalMinutes} min";
            if (tiempo.TotalHours < 24) return $"Falló hace {(int)tiempo.TotalHours} h";
            return $"Falló hace {(int)tiempo.TotalDays} d";
        }
    }
}
