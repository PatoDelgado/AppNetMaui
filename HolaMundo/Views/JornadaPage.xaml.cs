using HolaMundo.Models;
using HolaMundo.ViewModels;

namespace HolaMundo.Views;

public partial class JornadaPage : ContentPage
{
    public JornadaPage(JornadaViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        FormateaFecha();
    }

    private void FormateaFecha()
    {
        FechaLabel.Text = DateTime.Now.ToString("dddd d 'de' MMMM");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is JornadaViewModel vm)
        {
            await vm.RefrescaParadasCommand.ExecuteAsync(null);
            await vm.CargarResumenCommand.ExecuteAsync(null);
        }
    }

    private async void OnParadaTapped(object? sender, TappedEventArgs e)
    {
        if (sender is VisualElement { BindingContext: Parada parada })
        {
            await Shell.Current.GoToAsync($"{nameof(ParadaDetallePage)}?ParadaId={parada.Id}");
        }
    }
}
