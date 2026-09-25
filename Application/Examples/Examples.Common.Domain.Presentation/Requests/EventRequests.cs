namespace Examples.Common.Domain.Presentation.Requests;

public class CreateEventRequest
{
    public required DateTimeOffset HappensAt { get; init; }
}

public class UpdateEventDateRequest
{
    public required DateTimeOffset HappensAt { get; init; }
}