namespace Ukinee.Infrastructure.Ddd.Tests.Domain.Borders;

public class ExampleBorderFactory
{
    public static Border Example1 => new Border {
        Identifier = BorderIdentifier.Create(new Guid("cac7526f-9ef7-424c-9d0d-22312c43c73a")),
        Size = 123,
        IsAvailableForPublicRead = true,
    };

    public static Border Example2 => new Border {
        Identifier = BorderIdentifier.Create(new Guid("4a87bfdf-f0a8-4782-bea4-0d3546b1c652")),
        Size = 321,
        IsAvailableForPublicRead = false,
    };

    public static Border Example3 => new Border {
        Identifier = BorderIdentifier.Create(new Guid("c0eb0cfd-cc11-45f4-9f45-bdc886586e48")),
        Size = 999,
        IsAvailableForPublicRead = true,
    };

    public static CreateBorderRequest CreateRequest1 => new CreateBorderRequest {
        Size = Example1.Size,
    };

    public static CreateBorderRequest CreateRequest2 => new CreateBorderRequest {
        Size = Example2.Size,
    };

    public static CreateBorderRequest CreateRequest3 => new CreateBorderRequest {
        Size = Example3.Size,
    };

    public static UpdateBorderRequest UpdateRequest1 => new UpdateBorderRequest {
        Size = 8123,
    };

    public static UpdateBorderRequest UpdateRequest2 => new UpdateBorderRequest {
        Size = 8321,
    };

    public static UpdateBorderRequest UpdateRequest3 => new UpdateBorderRequest {
        Size = 8999,
    };
}
