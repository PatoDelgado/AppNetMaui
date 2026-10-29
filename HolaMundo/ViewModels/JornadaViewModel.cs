using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HolaMundo.Helpers;
using HolaMundo.Models;
using HolaMundo.Services;

namespace HolaMundo.ViewModels;

public partial class JornadaViewModel : ObservableObject
{
    private readonly ParadasDatabase _paradasDatabase;
    private readonly RepartidorDatabase _repartidorDatabase;
    private readonly TurnoDatabase _turnoDatabase;
    private readonly SesionActual _sesionActual;
    private readonly UbicacionService _ubicacionService;

    private List<Parada> _paradasCompletas = new();

    [ObservableProperty]
    private string _saludo = string.Empty;

    [ObservableProperty]
    private int _totalEntregas = 0;

    [ObservableProperty]
    private int _entregasCompletadas = 0;

    [ObservableProperty]
    private int _entregasEstaSemana = 0;

    [ObservableProperty]
    private int _entregasEsteMes = 0;

    [ObservableProperty]
    private bool _isRefrescando = false;

    [ObservableProperty]
    private string _mensajeEstado = string.Empty;

    [ObservableProperty]
    private Parada? _paradaSeleccionada;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextoBotonTurno))]
    private Turno? _turnoActivo;

    // Reto extra 6.1: Toggle para ocultar entregadas
    [ObservableProperty]
    private bool _ocultarEntregadas = false;

    // Reto opcional Ejercicio 7: Filtro por estado (Todas, Pendientes, Entregadas, Fallidas)
    [ObservableProperty]
    private string _filtroSeleccionado = "Todas";

    // Reto 6.5: Ordenar numéricamente por distancia (más cercana primero)
    [ObservableProperty]
    private bool _ordenarPorCercania = false;

    public ObservableCollection<string> OpcionesFiltro { get; } = new()
    {
        "Todas",
        "Pendientes",
        "Entregadas",
        "Fallidas"
    };

    public string TextoBotonTurno => TurnoActivo is null ? "Iniciar turno" : "Terminar turno";

    public ObservableCollection<Parada> Paradas { get; } = new();

    public JornadaViewModel(
        ParadasDatabase paradasDatabase,
        RepartidorDatabase repartidorDatabase,
        TurnoDatabase turnoDatabase,
        SesionActual sesionActual,
        UbicacionService ubicacionService)
    {
        _paradasDatabase = paradasDatabase;
        _repartidorDatabase = repartidorDatabase;
        _turnoDatabase = turnoDatabase;
        _sesionActual = sesionActual;
        _ubicacionService = ubicacionService;
    }

    partial void OnOcultarEntregadasChanged(bool value)
    {
        if (value && FiltroSeleccionado == "Entregadas")
        {
            FiltroSeleccionado = "Todas";
        }
        AplicarFiltroYOrden();
    }

    partial void OnFiltroSeleccionadoChanged(string value)
    {
        AplicarFiltroYOrden();
    }

    partial void OnOrdenarPorCercaniaChanged(bool value)
    {
        AplicarFiltroYOrden();
    }

    private void AplicarFiltroYOrden()
    {
        IEnumerable<Parada> filtradas = _paradasCompletas;

        // Filtro por Switch "Ocultar entregadas"
        if (OcultarEntregadas)
        {
            filtradas = filtradas.Where(p => p.Estado != EstadoEntrega.Entregada);
        }

        // Filtro por selector de estado (Reto Ejercicio 7)
        filtradas = FiltroSeleccionado switch
        {
            "Pendientes" => filtradas.Where(p => p.Estado == EstadoEntrega.Pendiente),
            "Entregadas" => filtradas.Where(p => p.Estado == EstadoEntrega.Entregada),
            "Fallidas"   => filtradas.Where(p => p.Estado == EstadoEntrega.Fallida),
            _            => filtradas,
        };

        // Ordenamiento numérico (Reto Ejercicio 6.5)
        IEnumerable<Parada> ordenadas;
        if (OrdenarPorCercania)
        {
            ordenadas = filtradas.OrderBy(p => p.DistanciaKm).ThenBy(p => p.Numero);
        }
        else
        {
            ordenadas = filtradas.OrderBy(p => p.Numero);
        }

        Paradas.Clear();
        foreach (var parada in ordenadas)
        {
            Paradas.Add(parada);
        }
    }

    [RelayCommand]
    private async Task RefrescaParadas()
    {
        if (_sesionActual.RepartidorId == 0)
        {
            await Shell.Current.GoToAsync("//LoginPage");
            return;
        }

        try
        {
            IsRefrescando = true;

            var repartidor = await _repartidorDatabase.GetRepartidorAsync(_sesionActual.RepartidorId);
            if (repartidor is null)
            {
                Preferences.Default.Remove("RepartidorActualId");
                _sesionActual.RepartidorId = 0;
                await Shell.Current.GoToAsync("//LoginPage");
                return;
            }

            FormarSaludo(repartidor.Nombre);
            TurnoActivo = await _turnoDatabase.GetTurnoAbiertoAsync();

#if DEBUG
            await Task.Delay(500);
#endif

            var paradas = await _paradasDatabase.GetParadasAsync();

            if (paradas.Count == 0)
            {
                MensajeEstado = "No hay paradas disponibles";
#if DEBUG
                await Task.Delay(1000);
                await InventaParadas();
                paradas = await _paradasDatabase.GetParadasAsync();
#endif
            }

            var miUbicacion = await _ubicacionService.ObtenerUbicacionActualAsync();

            if (miUbicacion is null)
            {
                MensajeEstado = "Activa la ubicación para ver distancias";
            }

            foreach (var p in paradas)
            {
                if (miUbicacion is not null && p.Latitud is not null && p.Longitud is not null)
                {
                    var km = Distancia.EnKilometros(
                        miUbicacion.Latitude, miUbicacion.Longitude,
                        p.Latitud.Value, p.Longitud.Value);
                    p.DistanciaKm = km;
                    p.DistanciaTexto = $"{km:F1} km";
                }
                else
                {
                    p.DistanciaKm = double.MaxValue;
                    p.DistanciaTexto = "Sin ubicación";
                }
            }

            _paradasCompletas = paradas;
            AplicarFiltroYOrden();

            TotalEntregas = _paradasCompletas.Count;
            EntregasCompletadas = _paradasCompletas.Count(p => p.Estado == EstadoEntrega.Entregada);

            if (miUbicacion is not null)
            {
                MensajeEstado = "¡Datos actualizados correctamente!";
            }
        }
        catch (Exception ex)
        {
            MensajeEstado = $"Error al actualizar datos: {ex.Message}";
        }
        finally
        {
            IsRefrescando = false;
        }

        await Task.Delay(3000);
        MensajeEstado = string.Empty;
    }

    [RelayCommand]
    private async Task AlternarTurnoAsync()
    {
        if (TurnoActivo is null)
        {
            await _turnoDatabase.IniciarTurnoAsync();
        }
        else
        {
            await _turnoDatabase.CerrarTurnoAsync(TurnoActivo);
        }

        TurnoActivo = await _turnoDatabase.GetTurnoAbiertoAsync();
    }

    [RelayCommand]
    private async Task CargarResumenAsync()
    {
        var desdeSemana = DateTime.Now.AddDays(-7);
        var desdeMes = DateTime.Now.AddDays(-30);
        var todas = await _paradasDatabase.GetParadasAsync();

        EntregasEstaSemana = todas.Count(p => p.Estado == EstadoEntrega.Entregada && p.ModificadoEn >= desdeSemana);
        EntregasEsteMes = todas.Count(p => p.Estado == EstadoEntrega.Entregada && p.ModificadoEn >= desdeMes);
    }

    // Reto 6.2: Cerrar sesión
    [RelayCommand]
    private async Task CerrarSesionAsync()
    {
        _sesionActual.RepartidorId = 0;
        await Shell.Current.GoToAsync("//LoginPage");
    }

    private async Task InventaParadas()
    {
        await _paradasDatabase.SaveParadaAsync(new Parada
        {
            Numero = 1,
            RepartidorId = _sesionActual.RepartidorId,
            Cliente = "Farmacia San Juan",
            Direccion = "Av. Hidalgo 123",
            Estado = EstadoEntrega.Entregada,
            Latitud = 20.6736,
            Longitud = -103.3440,
            ModificadoEn = DateTime.Now
        });

        await _paradasDatabase.SaveParadaAsync(new Parada
        {
            Numero = 2,
            RepartidorId = _sesionActual.RepartidorId,
            Cliente = "Abarrotes La Esquina",
            Direccion = "Calle Morelos 45",
            Estado = EstadoEntrega.Pendiente,
            Latitud = 20.6775,
            Longitud = -103.3490,
            ModificadoEn = DateTime.Now
        });

        await _paradasDatabase.SaveParadaAsync(new Parada
        {
            Numero = 3,
            RepartidorId = _sesionActual.RepartidorId,
            Cliente = "Consultorio Dra. Ruiz",
            Direccion = "Blvd. Constitucion 890",
            Estado = EstadoEntrega.Pendiente,
            Latitud = 20.6810,
            Longitud = -103.3550,
            ModificadoEn = DateTime.Now
        });

        await _paradasDatabase.SaveParadaAsync(new Parada
        {
            Numero = 4,
            RepartidorId = _sesionActual.RepartidorId,
            Cliente = "Don Panchito Perez",
            Direccion = "Ciudad Universitaria, Sistemas 890",
            Estado = EstadoEntrega.Pendiente,
            Latitud = 20.6550,
            Longitud = -103.3250,
            ModificadoEn = DateTime.Now
        });

        await _paradasDatabase.SaveParadaAsync(new Parada
        {
            Numero = 5,
            RepartidorId = _sesionActual.RepartidorId,
            Cliente = "Eulalio de la Cruz",
            Direccion = "Colon 888, Jalisco",
            Estado = EstadoEntrega.Entregada,
            Latitud = 20.6650,
            Longitud = -103.3600,
            ModificadoEn = DateTime.Now
        });
    }

    private void FormarSaludo(string nombreRepartidor)
    {
        string saludo;
        if (DateTime.Now.Hour < 12)
            saludo = "Buenos días ";
        else if (DateTime.Now.Hour < 20)
            saludo = "Buenas tardes ";
        else
            saludo = "Buenas noches ";

        Saludo = saludo + nombreRepartidor;
    }
}
