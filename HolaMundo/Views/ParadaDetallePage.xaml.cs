using HolaMundo.ViewModels;

namespace HolaMundo.Views;

public partial class ParadaDetallePage : ContentPage
{
    public ParadaDetallePage(ParadaDetalleViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
