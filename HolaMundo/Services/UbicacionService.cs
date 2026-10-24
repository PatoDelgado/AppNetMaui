namespace HolaMundo.Services;

public class UbicacionService
{
    public async Task<Location?> ObtenerUbicacionActualAsync()
    {
        try
        {
            var estado = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (estado != PermissionStatus.Granted)
                return null;

            var solicitud = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            return await Geolocation.Default.GetLocationAsync(solicitud);
        }
        catch
        {
            return null;
        }
    }
}
