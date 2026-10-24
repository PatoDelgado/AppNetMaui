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
    [NotifyCanExecuteChangedFor(nameof(MarcarEntregadaCommand))]
    [NotifyCanExecuteChangedFor(nameof(MarcarFallidaCommand))]
    [NotifyCanExecuteChangedFor(nameof(ReintentarEntregaCommand))]
    private Parada? _parada;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(MarcarFallidaCommand))]
    private string _razonFallo = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EsEditable))]
    [NotifyCanExecuteChangedFor(nameof(MarcarEntregadaCommand))]
    [NotifyCanExecuteChangedFor(nameof(MarcarFallidaCommand))]
    [NotifyCanExecuteChangedFor(nameof(ReintentarEntregaCommand))]
    private bool _isGuardando = false;

    [ObservableProperty]
    private string _mensajeEstado = string.Empty;

    public bool EsEntregada => Parada != null && Parada.Estado == "Entregada";
    public bool EsFallida => Parada != null && Parada.Estado == "Fallida";
    public bool EsEditable => Parada != null && Parada.Estado != "Entregada" && !IsGuardando;

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
        if (Parada != null && !string.IsNullOrEmpty(Parada.RazonFallo))
        {
            RazonFallo = Parada.RazonFallo;
        }
        else
        {
            RazonFallo = string.Empty;
        }

        OnPropertyChanged(nameof(EsEditable));
        OnPropertyChanged(nameof(EsFallida));
        OnPropertyChanged(nameof(EsEntregada));
        MarcarEntregadaCommand.NotifyCanExecuteChanged();
        MarcarFallidaCommand.NotifyCanExecuteChanged();
        ReintentarEntregaCommand.NotifyCanExecuteChanged();
    }

    private bool PuedeMarcarEntregada() => !IsGuardando && EsEditable;

    [RelayCommand(CanExecute = nameof(PuedeMarcarEntregada))]
    private async Task MarcarEntregadaAsync()
    {
        if (Parada is null || IsGuardando) return;

        try
        {
            IsGuardando = true;
            Parada.Estado = "Entregada";
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

    private bool PuedeMarcarFallida() => !IsGuardando && EsEditable && !string.IsNullOrWhiteSpace(RazonFallo);

    [RelayCommand(CanExecute = nameof(PuedeMarcarFallida))]
    private async Task MarcarFallidaAsync()
    {
        if (Parada is null || IsGuardando) return;

        if (string.IsNullOrWhiteSpace(RazonFallo))
        {
            MensajeEstado = "Escribe una razón antes de marcar el fallo.";
            return;
        }

        try
        {
            IsGuardando = true;
            Parada.Estado = "Fallida";
            Parada.RazonFallo = RazonFallo;
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

    private bool PuedeReintentar() => !IsGuardando && EsFallida;

    [RelayCommand(CanExecute = nameof(PuedeReintentar))]
    private async Task ReintentarEntregaAsync()
    {
        if (Parada is null || IsGuardando) return;

        try
        {
            IsGuardando = true;
            Parada.Estado = "Pendiente";
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
