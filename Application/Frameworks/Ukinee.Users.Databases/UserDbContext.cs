using Microsoft.EntityFrameworkCore;
using Ukinee.Users.Domain;

namespace Ukinee.Users.Databases;

public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.ComplexProperties<UserAccount>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        const string UsernameColumnName = "Account_Username";

        modelBuilder
            .Entity<User>()
            .ComplexProperty(u => u.Account)
            .Property(a => a.Username)
            .HasColumnName(UsernameColumnName);

        modelBuilder
            .Entity<User>()
            .HasIndex(UsernameColumnName)
            .IsUnique();

        modelBuilder.Entity<User>().HasKey(u => u.UserGuid);
    }
}
