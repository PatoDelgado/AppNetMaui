namespace HolaMundo.Services;

public class SesionActual
{
    // 0 = nadie ha iniciado sesión todavía.
    public int RepartidorId { get; set; } = Preferences.Default.Get("RepartidorActualId", 0);
}
