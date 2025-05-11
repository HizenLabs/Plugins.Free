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
}
