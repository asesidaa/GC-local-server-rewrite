using System.Numerics;
using System.Text.Json;

namespace Application.Common.Helpers;

/// <summary>
/// Operates on long[] arrays stored as JSON text in the database.
/// Each long = 64 bits. Item ID N (1-based) maps to array[(N-1)/64], bit (N-1)%64.
/// Bit = 1 means unlocked, bit = 0 means locked.
/// </summary>
public static class BitsetHelper
{
    public static int LongsRequired(int itemCount) => (itemCount + 63) / 64;

    public static long[] CreateAllZeroes(int itemCount) => new long[LongsRequired(itemCount)];

    public static long[] CreateAllOnes(int itemCount)
    {
        var count = LongsRequired(itemCount);
        var bitset = new long[count];
        Array.Fill(bitset, ~0L);

        // Mask off excess bits in the last element
        var excessBits = itemCount % 64;
        if (excessBits != 0 && count > 0)
        {
            bitset[count - 1] = (1L << excessBits) - 1;
        }

        return bitset;
    }

    public static bool IsUnlocked(long[] bitset, int itemId)
    {
        var index = (itemId - 1) / 64;
        if (index < 0 || index >= bitset.Length)
            return false;

        var bit = (itemId - 1) % 64;
        return (bitset[index] & (1L << bit)) != 0;
    }

    public static void SetBit(long[] bitset, int itemId, bool unlocked)
    {
        var index = (itemId - 1) / 64;
        if (index < 0 || index >= bitset.Length)
            return;

        var bit = (itemId - 1) % 64;
        if (unlocked)
            bitset[index] |= (1L << bit);
        else
            bitset[index] &= ~(1L << bit);
    }

    public static long[] EnsureLength(long[] bitset, int requiredCount)
    {
        var required = LongsRequired(requiredCount);
        if (bitset.Length >= required)
            return bitset;

        var grown = new long[required];
        bitset.CopyTo(grown, 0);
        // New elements default to 0 (locked)
        return grown;
    }

    public static int CountUnlocked(long[] bitset, int totalCount)
    {
        var count = 0;
        var fullLongs = totalCount / 64;

        for (var i = 0; i < fullLongs && i < bitset.Length; i++)
            count += BitOperations.PopCount((ulong)bitset[i]);

        // Partial last element
        var remainingBits = totalCount % 64;
        if (remainingBits > 0 && fullLongs < bitset.Length)
        {
            var mask = (1L << remainingBits) - 1;
            count += BitOperations.PopCount((ulong)(bitset[fullLongs] & mask));
        }

        return count;
    }

    public static string Serialize(long[] bitset) => JsonSerializer.Serialize(bitset);

    public static long[] Deserialize(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return [];

        return JsonSerializer.Deserialize<long[]>(json) ?? [];
    }
}
