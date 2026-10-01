using Examples.Common.Domain.Models.Identifiers;
using Ukinee.Infrastructure.Ddd.Tests.Utils.Mocks;

namespace Ukinee.Infrastructure.Ddd.Tests.Utils.Factories;

public class AttendanceFactory
{
    public static Announcement Example1 => new Announcement {
        Identifier = AnnouncementIdentifier.Create( new Guid("40e1fb4d-1c85-4d19-a756-8b670dd9a71b")),
        IsAvailableForPublicRead = true,
    };
    public static Announcement Example2 => new Announcement {
        Identifier = AnnouncementIdentifier.Create( new Guid("e2c4e7da-0b0b-4eea-9689-9f8f7791b491")),
        IsAvailableForPublicRead = false,
    };
}
