namespace HolaMundo.Helpers;

public static class Distancia
{
    public static double EnKilometros(double lat1, double lon1, double lat2, double lon2)
    {
        const double radioTierra = 6371; // km
        var dLat = ARadianes(lat2 - lat1);
        var dLon = ARadianes(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ARadianes(lat1)) * Math.Cos(ARadianes(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return radioTierra * c;
    }

    private static double ARadianes(double grados) => grados * Math.PI / 180;
}
