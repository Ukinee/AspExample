using System.ComponentModel.DataAnnotations;

namespace Ukinee.Infrastructure.Ddd.Local.EfCore.Domain;

public static class ChangeSequenceUtils<TTag>
{
    public static string TagName => typeof(TTag).Name;
    public static string TableName => "__ChangeSequence";
}

public class ChangeSequenceEntry
{
    [MaxLength(128)] public required string Tag { get; init; }

    public required long Value { get; init; }
}
