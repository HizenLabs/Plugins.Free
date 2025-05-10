using HizenLabs.Extensions.UserPreference.Material.Structs;
using System.Runtime.CompilerServices;

namespace HizenLabs.Extensions.UserPreference.Material.Utils;

public static class StringUtils
{
    /// <summary>
    /// Converts a hex color argb value to its hex string representation.
    /// </summary>
    /// <param name="argb">The ARGB color value.</param>
    /// <returns>The hex string representation of the color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string HexFromArgb(ColorArgb color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}
