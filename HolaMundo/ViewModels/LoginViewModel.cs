using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using HolaMundo.Models;
using HolaMundo.Services;

namespace HolaMundo.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly RepartidorDatabase _repartidorDatabase;

    public ObservableCollection<Repartidor> Repartidores { get; } = new();

    public LoginViewModel(RepartidorDatabase repartidorDatabase)
    {
        _repartidorDatabase = repartidorDatabase;
    }

    public async Task CargarAsync()
    {
        var repartidores = await _repartidorDatabase.GetRepartidoresAsync();
        Repartidores.Clear();
        foreach (var r in repartidores)
        {
            Repartidores.Add(r);
        }
    }
}
