using SQLite;
using HolaMundo.Models;

namespace HolaMundo.Services;

public class ParadasDatabase
{
    private readonly SesionActual _sesion;
    private SQLiteAsyncConnection? _database;
    private readonly string _dbPath;

    public ParadasDatabase(SesionActual sesion)
    {
        _sesion = sesion;
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "paradasUltMill.db3");
    }

    private async Task InitAsync()
    {
        if (_database is not null) return;

        _database = new SQLiteAsyncConnection(_dbPath);
        await _database.CreateTableAsync<Parada>();
    }

    public async Task<List<Parada>> GetParadasAsync()
    {
        await InitAsync();
        var repartidorId = _sesion.RepartidorId;
        return await _database!.Table<Parada>()
            .Where(p => p.RepartidorId == repartidorId)
            .OrderBy(p => p.Numero)
            .ToListAsync();
    }

    public async Task<Parada?> GetParadaAsync(Parada parada)
    {
        await InitAsync();
        return await _database!.Table<Parada>()
            .Where(p => p.Id == parada.Id).FirstOrDefaultAsync();
    }

    public async Task<Parada?> GetParadaByIdAsync(int id)
    {
        await InitAsync();
        return await _database!.Table<Parada>()
            .Where(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task<int> SaveParadaAsync(Parada parada)
    {
        await InitAsync();
        if (parada.Id != 0)
            return await _database!.UpdateAsync(parada);
        else
            return await _database!.InsertAsync(parada);
    }
}
