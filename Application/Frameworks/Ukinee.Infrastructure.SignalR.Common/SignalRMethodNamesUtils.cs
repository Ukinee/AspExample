namespace Ukinee.Infrastructure.SignalR.Common;

public static class SignalRMethodNamesUtils
{
    public static string Created<T>()
    {
        return $"{typeof(T).Name}OnCreated";
    }

    public static string Updated<T>()
    {
        return $"{typeof(T).Name}OnUpdated";
    }

    public static string Removed<T>()
    {
        return $"{typeof(T).Name}OnUpdated";
    }

    public static string Upsert<T>()
    {
        return $"{typeof(T).Name}BatchUpsert";
    }

    public static string Subscribe<T>()
    {
        return $"Subscribe{typeof(T).Name}";
    }

    public static string Unsubscribe<T>()
    {
        return $"Unsubscribe{typeof(T).Name}";
    }
}
