using Examples.Common.Domain.Models.Identifiers;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;

namespace Ukinee.Infrastructure.Ddd.Tests.Utils.Mocks;

public readonly record struct AnnouncementIdentifier
{
    public required Guid Guid { get; init; }

    public static AnnouncementIdentifier Create(Guid guid)
    {
        return new AnnouncementIdentifier { Guid = guid };
    }
}

public class Announcement : IEntity<AnnouncementIdentifier>, IEntityWithPublicRead<Announcement>
{
    public required AnnouncementIdentifier Identifier { get; init; }
    public required bool IsAvailableForPublicRead { get; init; }

    public Announcement OpenPublicRead()
    {
        throw new NotImplementedException();
    }

    public Announcement ClosePublicRead()
    {
        throw new NotImplementedException();
    }
}
