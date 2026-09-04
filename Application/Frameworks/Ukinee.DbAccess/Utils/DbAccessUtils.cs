using System.Globalization;

namespace Ukinee.DbAccess.Utils;

internal class DbAccessUtils
{
    internal static TIdentifier Parse<TIdentifier>(string? str)
    where TIdentifier : ISpanParsable<TIdentifier>
    {
        return TIdentifier.Parse(str, CultureInfo.InvariantCulture);
    }
}
