using HolaMundo.Models;
using HolaMundo.Services;
using HolaMundo.ViewModels;

namespace HolaMundo.Views;

public partial class LoginPage : ContentPage
{
    private readonly SesionActual _sesionActual;

    public LoginPage(LoginViewModel vm, SesionActual sesionActual)
    {
        InitializeComponent();
        BindingContext = vm;
        _sesionActual = sesionActual;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is LoginViewModel vm)
            await vm.CargarAsync();
    }

    private async void OnRepartidorSeleccionado(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Repartidor repartidor)
        {
            Preferences.Default.Set("RepartidorActualId", repartidor.Id);
            _sesionActual.RepartidorId = repartidor.Id;
            await Shell.Current.GoToAsync($"//{nameof(JornadaPage)}");
        }
    }
}
