namespace Ukinee.Infrastructure.Ddd.Local.EfCore.Options;

// ReSharper disable once UnusedTypeParameter
public class DbConnectionSettingsCollection<TContext>
{
    public DbConnectionSettings[] Items { get; set; } = [];
}
