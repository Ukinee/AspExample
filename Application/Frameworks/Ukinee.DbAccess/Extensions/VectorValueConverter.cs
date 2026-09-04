using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Ukinee.DbAccess.Extensions;

public class VectorValueConverter() : ValueConverter<float[], byte[]>(
    v => ToBytes(v),
    v => ToFloats(v)
)
{
    private static float[] ToFloats(byte[] v) =>
        MemoryMarshal
            .Cast<byte, float>(v.AsSpan())
            .ToArray();

    private static byte[] ToBytes(float[] v) =>
        MemoryMarshal
            .AsBytes(v.AsSpan())
            .ToArray();
}

public class VectorValueComparer : ValueComparer<float[]?>
{
    public VectorValueComparer() : base(
        (l, r) => Compare(l, r),
        v => CalculateHash(v),
        v => v == null ? null : (float[])v.Clone()
    ) { }

    private static bool Compare(float[]? l, float[]? r)
    {
        if (l == null && r == null)
            return true;

        return l != null && r != null && l
            .AsSpan()
            .SequenceEqual(r.AsSpan());
    }

    private static int CalculateHash(float[]? v)
    {
        var hash = new HashCode();
        hash.AddBytes(MemoryMarshal.AsBytes(v.AsSpan()));

        return hash.ToHashCode();
    }
}
