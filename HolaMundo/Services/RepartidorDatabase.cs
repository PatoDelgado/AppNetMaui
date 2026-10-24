using SQLite;
using HolaMundo.Models;

namespace HolaMundo.Services;

public class RepartidorDatabase
{
    private SQLiteAsyncConnection? _database;
    private readonly string _dbPath;

    public RepartidorDatabase()
    {
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "paradasUltMill.db3");
    }

    private async Task InitAsync()
    {
        if (_database is not null) return;
        _database = new SQLiteAsyncConnection(_dbPath);
        await _database.CreateTableAsync<Repartidor>();
    }

    public async Task<List<Repartidor>> GetRepartidoresAsync()
    {
        await InitAsync();
        var repartidores = await _database!.Table<Repartidor>().ToListAsync();

        if (repartidores.Count == 0)
        {
            await _database.InsertAsync(new Repartidor { Nombre = "Miguel Torres" });
            await _database.InsertAsync(new Repartidor { Nombre = "Ana Delgado" });
            repartidores = await _database.Table<Repartidor>().ToListAsync();
        }

        return repartidores;
    }

    public async Task<Repartidor?> GetRepartidorAsync(int id)
    {
        await InitAsync();
        return await _database!.Table<Repartidor>()
            .Where(r => r.Id == id).FirstOrDefaultAsync();
    }
}
