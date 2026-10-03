using System.Linq.Expressions;
using Ukinee.Infrastructure.Ddd.Local.InMemory.Repositories;
using Ukinee.Infrastructure.Ddd.Local.Repositories;
using Ukinee.Infrastructure.Ddd.Tests.Common.Services.Repositories;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Announcements;

namespace Ukinee.Infrastructure.Ddd.Tests.Common.Utils.TestBases;

public abstract class AnnouncementTestBase : TestBase
{
    protected IEditableTrackedRepository<AnnouncementIdentifier, Announcement> Repository => GetService<IEditableTrackedRepository<AnnouncementIdentifier, Announcement>>();
    protected InMemoryStore<AnnouncementIdentifier, Announcement> Store => GetService<InMemoryStore<AnnouncementIdentifier, Announcement>>();

    protected static Announcement Example1 => ExampleAnnouncementFactory.Example1;
    protected static AnnouncementIdentifier Identifier1 => ExampleAnnouncementFactory.Example1.Identifier;

    protected static Announcement Example2 => ExampleAnnouncementFactory.Example2;
    protected static AnnouncementIdentifier Identifier2 => ExampleAnnouncementFactory.Example2.Identifier;

    protected static Announcement Example3 => ExampleAnnouncementFactory.Example3;
    protected static AnnouncementIdentifier Identifier3 => ExampleAnnouncementFactory.Example3.Identifier;

    protected static CancellationToken CancellationToken => CancellationToken.None;

    protected static Expression<Func<Announcement, bool>> True => _ => true;
    protected static Expression<Func<Announcement, bool>> False => _ => false;
}
