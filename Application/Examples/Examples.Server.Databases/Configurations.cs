using Examples.Common.Domain.Models.Entities;
using Examples.Common.Domain.Models.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ukinee.DbAccess.Extensions;

namespace Examples.Server.Databases;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("events");
        builder.ConfigureComplexIdentifierKey<EventIdentifier, Event>();
    }
}

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("reviews");
        builder.ConfigureComplexIdentifierKey<ReviewIdentifier, Review>();
    }
}

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");
        builder.ConfigureComplexIdentifierKey<LocationIdentifier, Location>();
    }
}

public class LocationCategoryConfiguration : IEntityTypeConfiguration<LocationCategory>
{
    public void Configure(EntityTypeBuilder<LocationCategory> builder)
    {
        builder.ToTable("location_categories");
        builder.ConfigureComplexIdentifierKey<LocationCategoryIdentifier, LocationCategory>();
    }
}

public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder.ToTable("attendances");
        builder.ConfigureComplexIdentifierKey<AttendanceIdentifier, Attendance>();
    }
}
