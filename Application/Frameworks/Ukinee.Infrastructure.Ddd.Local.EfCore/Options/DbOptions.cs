using Microsoft.Extensions.Options;

namespace Ukinee.Infrastructure.Ddd.Local.EfCore.Options;

public class DbOptions<TContext>
{
    private readonly IOptions<DbConnectionSettingsCollection<TContext>> _connectionSettingsCollectionOptions;

    public DbOptions(IOptions<DbConnectionSettingsCollection<TContext>> connectionSettingsCollectionOptions)
    {
        _connectionSettingsCollectionOptions = connectionSettingsCollectionOptions;
    }

    private DbConnectionSettingsCollection<TContext> ConnectionSettingsCollection => _connectionSettingsCollectionOptions.Value;

    public string GetConnectionString(string database)
    {
        var settings = ConnectionSettingsCollection
            .Items
            .FirstOrDefault(item => string.Equals(database, item.Database, StringComparison.OrdinalIgnoreCase));

        if (settings == null)
            throw new InvalidOperationException("No connection settings found for database: " + database);

        return $"Host={settings.Host};Port={settings.Port};Database={database};Username={settings.Username};Password={settings.Password}";
    }
}
