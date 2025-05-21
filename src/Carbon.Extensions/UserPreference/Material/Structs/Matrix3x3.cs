using System;
using System.Runtime.InteropServices;

namespace HizenLabs.Extensions.UserPreference.Material.Structs;

/// <summary>
/// Represents a 3x3 matrix.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal readonly struct Matrix3x3
{
    public readonly double
        m00, m01, m02,
        m10, m11, m12,
        m20, m21, m22;

    /// <summary>
    /// Gets or sets the matrix element by linear index [0-8].
    /// </summary>
    /// <param name="index"> The index of the element, ranging from 0 (m00) to 8 (m22).</param>
    /// <returns>The float value at the specified index.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown if the index is not in the range 0–8.</exception>
    public double this[int index]
    {
        get => index switch
        {
            0 => m00,
            1 => m01,
            2 => m02,
            3 => m10,
            4 => m11,
            5 => m12,
            6 => m20,
            7 => m21,
            8 => m22,
            _ => throw new IndexOutOfRangeException("Invalid matrix index!"),
        };
    }

    /// <summary>
    /// Gets or sets the matrix element at the specified row and column.
    /// </summary>
    /// <param name="row">The row index (0–2).</param>
    /// <param name="column">The column index (0–2).</param>
    /// <returns>The float value at the specified (row, column) position.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown if either <paramref name="row"/> or <paramref name="column"/> is outside the range 0–2.</exception>
    public double this[int row, int column]
    {
        get => this[row * 3 + column];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Matrix3x3"/> struct using individual float values.
    /// </summary>
    /// <param name="m00">Value at row 0, column 0.</param>
    /// <param name="m01">Value at row 0, column 1.</param>
    /// <param name="m02">Value at row 0, column 2.</param>
    /// <param name="m10">Value at row 1, column 0.</param>
    /// <param name="m11">Value at row 1, column 1.</param>
    /// <param name="m12">Value at row 1, column 2.</param>
    /// <param name="m20">Value at row 2, column 0.</param>
    /// <param name="m21">Value at row 2, column 1.</param>
    /// <param name="m22">Value at row 2, column 2.</param>
    public Matrix3x3(
        double m00, double m01, double m02,
        double m10, double m11, double m12,
        double m20, double m21, double m22)
    {
        this.m00 = m00;
        this.m01 = m01;
        this.m02 = m02;
        this.m10 = m10;
        this.m11 = m11;
        this.m12 = m12;
        this.m20 = m20;
        this.m21 = m21;
        this.m22 = m22;
    }

    /// <summary>
    /// Computes the inverse of this 3x3 matrix.
    /// </summary>
    /// <returns>A new <see cref="Matrix3x3"/> representing the inverse.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the matrix is not invertible (determinant is zero).</exception>
    /// <remarks>
    /// Given a matrix:
    /// <code>
    /// | m00 | m01 | m02 |
    /// | m10 | m11 | m12 |
    /// | m20 | m21 | m22 |
    /// </code>
    /// We can get the a-i values as:
    /// <code>
    /// | a | b | c |
    /// | d | e | f |
    /// | g | h | i |
    /// </code>
    /// And then compute the determinant using the formula:
    /// <code>
    /// det(M) = a(ei − fh) − b(di − fg) + c(dh − eg)
    /// </code>
    /// Finally, the inverse is computed using the adjugate matrix and the determinant:
    /// <code>
    /// inv(M) = (1 / det(M) ) * adj(M)
    /// </code>
    /// </remarks>
    public Matrix3x3 ToInverted()
    {
        // Extract the matrix variables
        double a = m00, b = m01, c = m02,
               d = m10, e = m11, f = m12,
               g = m20, h = m21, i = m22;

        // Calculate the determinant
        double det = a * (e * i - f * h) - b * (d * i - f * g) + c * (d * h - e * g);

        // If determinant is too close to zero, matrix is not invertible
        if (Math.Abs(det) < 1e-12)
        {
            throw new InvalidOperationException("Matrix is not invertible, determinant is too small.");
        }

        double invDet = 1.0 / det;

        // Compute the inverse using the adjugate matrix and determinant
        return new Matrix3x3
        (
            invDet * (e * i - f * h),
            invDet * (c * h - b * i),
            invDet * (b * f - c * e),
            invDet * (f * g - d * i),
            invDet * (a * i - c * g),
            invDet * (c * d - a * f),
            invDet * (d * h - e * g),
            invDet * (b * g - a * h),
            invDet * (a * e - b * d)
        );
    }


    /// <summary>
    /// Multiplies a 3x3 matrix by a 3D vector.
    /// </summary>
    /// <param name="matrix">The 3x3 matrix.</param>
    /// <param name="colorXyz">The 3D vector.</param>
    /// <returns>The resulting 3D vector after multiplication.</returns>
    public static Vector3d operator *(Matrix3x3 matrix, Vector3d colorXyz)
    {
        return new
        (
            matrix.m00 * colorXyz.X + matrix.m01 * colorXyz.Y + matrix.m02 * colorXyz.Z,
            matrix.m10 * colorXyz.X + matrix.m11 * colorXyz.Y + matrix.m12 * colorXyz.Z,
            matrix.m20 * colorXyz.X + matrix.m21 * colorXyz.Y + matrix.m22 * colorXyz.Z
        );
    }

    /// <inheritdoc cref="operator *(Matrix3x3, CieXyz)"/>
    public static Vector3d operator *(Vector3d colorXyz, Matrix3x3 matrix)
    {
        return matrix * colorXyz;
    }

    public static Vector3i operator *(Matrix3x3 matrix, Vector3i colorXyz)
    {
        return new
        (
            (int)(matrix.m00 * colorXyz.x + matrix.m01 * colorXyz.y + matrix.m02 * colorXyz.z),
            (int)(matrix.m10 * colorXyz.x + matrix.m11 * colorXyz.y + matrix.m12 * colorXyz.z),
            (int)(matrix.m20 * colorXyz.x + matrix.m21 * colorXyz.y + matrix.m22 * colorXyz.z)
        );
    }

    public static Vector3i operator *(Vector3i colorXyz, Matrix3x3 matrix)
    {
        return matrix * colorXyz;
    }
}
