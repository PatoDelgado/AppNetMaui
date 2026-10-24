using SQLite;
using HolaMundo.Models;

namespace HolaMundo.Services;

public class TurnoDatabase
{
    private readonly SesionActual _sesion;
    private SQLiteAsyncConnection? _database;
    private readonly string _dbPath;

    public TurnoDatabase(SesionActual sesion)
    {
        _sesion = sesion;
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "paradasUltMill.db3");
    }

    private async Task InitAsync()
    {
        if (_database is not null) return;
        _database = new SQLiteAsyncConnection(_dbPath);
        await _database.CreateTableAsync<Turno>();
    }

    public async Task<Turno?> GetTurnoAbiertoAsync()
    {
        await InitAsync();
        var repartidorId = _sesion.RepartidorId;
        return await _database!.Table<Turno>()
            .Where(t => t.RepartidorId == repartidorId && t.FechaFin == null)
            .FirstOrDefaultAsync();
    }

    public async Task<int> IniciarTurnoAsync()
    {
        await InitAsync();
        return await _database!.InsertAsync(new Turno
        {
            RepartidorId = _sesion.RepartidorId,
            FechaInicio = DateTime.Now,
        });
    }

    public async Task<int> CerrarTurnoAsync(Turno turno)
    {
        await InitAsync();
        turno.FechaFin = DateTime.Now;
        return await _database!.UpdateAsync(turno);
    }
}
