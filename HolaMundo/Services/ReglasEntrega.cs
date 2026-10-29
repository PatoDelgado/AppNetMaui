using HolaMundo.Models;

namespace HolaMundo.Services;

public static class ReglasEntrega
{
    public static bool PuedeEntregar(Parada p) =>
        p.Estado == EstadoEntrega.Pendiente;

    public static bool PuedeFallar(Parada p) =>
        p.Estado == EstadoEntrega.Pendiente;

    public static bool PuedeReintentar(Parada p) =>
        p.Estado == EstadoEntrega.Fallida;
}
