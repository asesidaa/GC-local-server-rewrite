namespace WebUI.Common;

/// <summary>
/// Client-side bitset utilities matching the server-side BitsetHelper format.
/// Bitset is a long[] where each long = 64 bits. Item ID N (1-based) maps to index (N-1)/64, bit (N-1)%64.
/// </summary>
public static class BitsetUtils
{
    public static bool IsUnlocked(long[] bitset, int itemId)
    {
        if (itemId < 1) return false;
        var index = (itemId - 1) / 64;
        var bit = (itemId - 1) % 64;
        if (index >= bitset.Length) return false;
        return (bitset[index] & (1L << bit)) != 0;
    }
}
