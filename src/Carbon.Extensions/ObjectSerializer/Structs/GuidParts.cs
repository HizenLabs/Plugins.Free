using System.Runtime.InteropServices;

namespace HizenLabs.Extensions.ObjectSerializer.Structs;

/// <summary>
/// Represents the parts of a <see cref="Guid"/> structure.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct GuidParts
{
    public int _a;
    public short _b;
    public short _c;
    public byte _d;
    public byte _e;
    public byte _f;
    public byte _g;
    public byte _h;
    public byte _i;
    public byte _j;
    public byte _k;
}
