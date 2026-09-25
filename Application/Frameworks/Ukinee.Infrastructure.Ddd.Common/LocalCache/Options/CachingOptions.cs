namespace Ukinee.Infrastructure.Ddd.Common.LocalCache.Options;

public class CachingOptions
{
    public required int CacheDurationSeconds { get; set; }
    
    public TimeSpan CacheDuration => TimeSpan.FromSeconds(CacheDurationSeconds);
}
