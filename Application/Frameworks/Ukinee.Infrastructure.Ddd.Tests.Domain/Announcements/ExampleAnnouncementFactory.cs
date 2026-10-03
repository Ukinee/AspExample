using Ukinee.Infrastructure.Ddd.Tests.Domain.Users;

namespace Ukinee.Infrastructure.Ddd.Tests.Domain.Announcements;

public class ExampleAnnouncementFactory
{
    public static Announcement Example1 => new Announcement {
        Identifier = AnnouncementIdentifier.Create(ExampleUserFactory.User1, new Guid("40e1fb4d-1c85-4d19-a756-8b670dd9a71b")),
        IsAvailableForPublicRead = true,
        Contents = "DADADADADADADADADAD",
    };

    public static Announcement Example2 => new Announcement {
        Identifier = AnnouncementIdentifier.Create(ExampleUserFactory.User2, new Guid("e2c4e7da-0b0b-4eea-9689-9f8f7791b491")),
        IsAvailableForPublicRead = false,
        Contents = "AAAAAAAA",
    };

    public static Announcement Example3 => new Announcement {
        Identifier = AnnouncementIdentifier.Create(ExampleUserFactory.User3, new Guid("e2c4e7da-0b0b-4eea-9689-9f8f7791b491")),
        IsAvailableForPublicRead = true,
        Contents = "23312123312321",
    };

    public static CreateAnnouncementRequest CreateRequest1 => new CreateAnnouncementRequest {
        Contents = Example1.Contents,
    };

    public static CreateAnnouncementRequest CreateRequest2 => new CreateAnnouncementRequest {
        Contents = Example2.Contents,
    };

    public static CreateAnnouncementRequest CreateRequest3 => new CreateAnnouncementRequest {
        Contents = Example3.Contents,
    };

    public static UpdateAnnouncementRequest UpdateRequest1 => new UpdateAnnouncementRequest {
        Contents = "UpdatedAnnouncementRequest1",
    };

    public static UpdateAnnouncementRequest UpdateRequest2 => new UpdateAnnouncementRequest {
        Contents = "UpdatedAnnouncementRequest2",
    };

    public static UpdateAnnouncementRequest UpdateRequest3 => new UpdateAnnouncementRequest {
        Contents = "UpdatedAnnouncementRequest3",
    };
}
