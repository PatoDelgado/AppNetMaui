using HolaMundo.ViewModels;

namespace HolaMundo.Views;

public partial class HistoricoPage : ContentPage
{
    private readonly HistoricoViewModel _vm;

    public HistoricoPage(HistoricoViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.CargarHistoricoMesCommand.ExecuteAsync(null);
    }
}
