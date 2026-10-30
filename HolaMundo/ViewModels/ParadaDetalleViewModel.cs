using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HolaMundo.Models;
using HolaMundo.Services;

namespace HolaMundo.ViewModels;

[QueryProperty(nameof(ParadaId), "ParadaId")]
public partial class ParadaDetalleViewModel : ObservableObject
{
    private readonly ParadasDatabase _paradasDatabase;

    [ObservableProperty]
    private int _paradaId;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EsEditable))]
    [NotifyPropertyChangedFor(nameof(EsFallida))]
    [NotifyPropertyChangedFor(nameof(EsEntregada))]
    [NotifyPropertyChangedFor(nameof(TiempoEnFallo))]
    [NotifyPropertyChangedFor(nameof(TieneIntentosFallidos))]
    [NotifyCanExecuteChangedFor(nameof(MarcarEntregadaCommand))]
    [NotifyCanExecuteChangedFor(nameof(MarcarFallidaCommand))]
    [NotifyCanExecuteChangedFor(nameof(ReintentarCommand))]
    private Parada? _parada;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(MarcarFallidaCommand))]
    private string _razonFallo = string.Empty;

    // N9: Guardar quién recibió el paquete al entregar
    [ObservableProperty]
    private string _quienRecibio = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EsEditable))]
    [NotifyCanExecuteChangedFor(nameof(MarcarEntregadaCommand))]
    [NotifyCanExecuteChangedFor(nameof(MarcarFallidaCommand))]
    [NotifyCanExecuteChangedFor(nameof(ReintentarCommand))]
    private bool _isGuardando = false;

    [ObservableProperty]
    private string _mensajeEstado = string.Empty;

    public bool EsEntregada => Parada != null && Parada.Estado == EstadoEntrega.Entregada;
    public bool EsFallida => Parada != null && Parada.Estado == EstadoEntrega.Fallida;
    public bool EsEditable => Parada != null && Parada.Estado != EstadoEntrega.Entregada && !IsGuardando;
    public string TiempoEnFallo => Parada?.TiempoEnFalloTexto ?? string.Empty;
    public bool TieneIntentosFallidos => Parada != null && Parada.IntentosFallidos > 0;

    public ParadaDetalleViewModel(ParadasDatabase paradasDatabase)
    {
        _paradasDatabase = paradasDatabase;
    }

    partial void OnParadaIdChanged(int value)
    {
        _ = CargarAsync();
    }

    private async Task CargarAsync()
    {
        Parada = await _paradasDatabase.GetParadaByIdAsync(ParadaId);
        if (Parada != null)
        {
            RazonFallo = Parada.RazonFallo ?? string.Empty;
            QuienRecibio = Parada.QuienRecibio ?? string.Empty;
        }
        else
        {
            RazonFallo = string.Empty;
            QuienRecibio = string.Empty;
        }

        OnPropertyChanged(nameof(EsEditable));
        OnPropertyChanged(nameof(EsFallida));
        OnPropertyChanged(nameof(EsEntregada));
        OnPropertyChanged(nameof(TiempoEnFallo));
        OnPropertyChanged(nameof(TieneIntentosFallidos));
        MarcarEntregadaCommand.NotifyCanExecuteChanged();
        MarcarFallidaCommand.NotifyCanExecuteChanged();
        ReintentarCommand.NotifyCanExecuteChanged();
    }

    private bool PuedeMarcarEntregada() =>
        !IsGuardando && Parada is not null && ReglasEntrega.PuedeEntregar(Parada);

    [RelayCommand(CanExecute = nameof(PuedeMarcarEntregada))]
    private async Task MarcarEntregadaAsync()
    {
        if (Parada is null || IsGuardando) return;

        if (!ReglasEntrega.PuedeEntregar(Parada))
        {
            MensajeEstado = "Esta parada ya no está pendiente.";
            return;
        }

        try
        {
            IsGuardando = true;
            Parada.Estado = EstadoEntrega.Entregada;
            // N9: Asignar quién recibió el paquete
            Parada.QuienRecibio = string.IsNullOrWhiteSpace(QuienRecibio) ? Parada.Cliente : QuienRecibio.Trim();
            Parada.ModificadoEn = DateTime.Now;
            await _paradasDatabase.SaveParadaAsync(Parada);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            MensajeEstado = $"Error al guardar: {ex.Message}";
        }
        finally
        {
            IsGuardando = false;
        }
    }

    private bool PuedeMarcarFallida() =>
        !IsGuardando && Parada is not null && ReglasEntrega.PuedeFallar(Parada) && !string.IsNullOrWhiteSpace(RazonFallo);

    // N3: Confirmar antes de marcar fallida
    [RelayCommand(CanExecute = nameof(PuedeMarcarFallida))]
    private async Task MarcarFallidaAsync()
    {
        if (Parada is null || IsGuardando) return;

        if (!ReglasEntrega.PuedeFallar(Parada))
        {
            MensajeEstado = "Esta parada ya no está pendiente.";
            return;
        }

        if (string.IsNullOrWhiteSpace(RazonFallo))
        {
            MensajeEstado = "Escribe una razón antes de marcar el fallo.";
            return;
        }

        // N3: Cuadro de diálogo de confirmación
        bool confirmar = await Shell.Current.DisplayAlert(
            "Confirmación",
            "¿Estás seguro de que deseas reportar esta parada como FALLIDA?",
            "Sí, reportar",
            "Cancelar");

        if (!confirmar) return;

        try
        {
            IsGuardando = true;
            Parada.Estado = EstadoEntrega.Fallida;
            Parada.RazonFallo = RazonFallo;
            // N11: Incrementar contador de intentos fallidos
            Parada.IntentosFallidos++;
            Parada.ModificadoEn = DateTime.Now;
            await _paradasDatabase.SaveParadaAsync(Parada);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            MensajeEstado = $"Error al guardar: {ex.Message}";
        }
        finally
        {
            IsGuardando = false;
        }
    }

    private bool PuedeReintentar() =>
        !IsGuardando && Parada is not null && ReglasEntrega.PuedeReintentar(Parada);

    [RelayCommand(CanExecute = nameof(PuedeReintentar))]
    private async Task ReintentarAsync()
    {
        if (Parada is null || IsGuardando) return;

        if (!ReglasEntrega.PuedeReintentar(Parada))
        {
            MensajeEstado = "Esa parada no está fallida.";
            return;
        }

        try
        {
            IsGuardando = true;
            Parada.Estado = EstadoEntrega.Pendiente;
            Parada.RazonFallo = null;
            RazonFallo = string.Empty;
            Parada.ModificadoEn = DateTime.Now;
            await _paradasDatabase.SaveParadaAsync(Parada);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            MensajeEstado = $"Error al guardar: {ex.Message}";
        }
        finally
        {
            IsGuardando = false;
        }
    }
}
