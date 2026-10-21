using Microsoft.EntityFrameworkCore;
using Ukinee.DbAccess.Extensions;
using Ukinee.Infrastructure.Ddd.Local.EfCore.Services;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Announcements;
using Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Server.Extensions;

namespace Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Server.Services;

public class ServerDbContext(DbContextOptions<ServerDbContext> options) : TaggedDbContext<ServerDbContext, ServerTag>(options)
{
    public DbSet<Announcement> Announcements { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder
            .Entity<Announcement>()
            .ToTable("announcements")
            .ConfigureComplexIdentifierKey<AnnouncementIdentifier, Announcement>();
    }
}
