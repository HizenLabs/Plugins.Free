using System;
using System.Runtime.CompilerServices;

namespace HizenLabs.Extensions.UserPreference.Material.Structs;

public struct Vector3d
{
    public double X { get; set; }

    public double Y { get; set; }

    public double Z { get; set; }

    public Vector3d(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public readonly double this[int index]
    {
        get
        {
            return index switch
            {
                0 => X,
                1 => Y,
                2 => Z,
                _ => throw new IndexOutOfRangeException("Invalid Vector3d index!")
            };
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly double Sum()
    {
        return X + Y + Z;
    }

    public static Vector3d operator *(Vector3d scalar, Vector3d vector)
    {
        return new Vector3d
        (
            scalar.X * vector.X,
            scalar.Y * vector.Y,
            scalar.Z * vector.Z
        );
    }
}
