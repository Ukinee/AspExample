using Examples.Common.Domain.Models.Entities;
using Examples.Server.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Ukinee.DbAccess.Extensions;
using Ukinee.Infrastructure.Ddd.Local.EfCore.Services;

namespace Examples.Server.Databases;

public class ServerDatabaseContext(DbContextOptions<ServerDatabaseContext> options) : TaggedDbContext<ServerDatabaseContext, ServerExampleTag>(options)
{
    public DbSet<Event> EventSet { get; set; }
    public DbSet<Review> ReviewSet { get; set; }
    public DbSet<Location> LocationSet { get; set; }
    public DbSet<LocationCategory> LocationCategorySet { get; set; }
    public DbSet<Attendance> AttendanceSet { get; set; }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.ApplyGlobalConventions();
        configurationBuilder.ApplyServerDomainConventions();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new EventConfiguration());
        modelBuilder.ApplyConfiguration(new ReviewConfiguration());
        modelBuilder.ApplyConfiguration(new LocationConfiguration());
        modelBuilder.ApplyConfiguration(new LocationCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new AttendanceConfiguration());
    }
}
