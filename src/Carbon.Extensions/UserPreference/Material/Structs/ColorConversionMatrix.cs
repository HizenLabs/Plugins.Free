using System.Runtime.InteropServices;

namespace HizenLabs.Extensions.UserPreference.Material.Structs;

/// <summary>
/// Represents a 3x3 matrix.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct ColorConversionMatrix
{
    public readonly double m00, m01, m02;
    public readonly double m10, m11, m12;
    public readonly double m20, m21, m22;

    /// <summary>
    /// Gets or sets the matrix element by linear index [0-8].
    /// </summary>
    /// <param name="index"> The index of the element, ranging from 0 (m00) to 8 (m22).</param>
    /// <returns>The float value at the specified index.</returns>
    /// <exception cref="System.IndexOutOfRangeException">Thrown if the index is not in the range 0–8.</exception>
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
            _ => throw new System.IndexOutOfRangeException("Invalid matrix index!"),
        };
    }

    /// <summary>
    /// Gets or sets the matrix element at the specified row and column.
    /// </summary>
    /// <param name="row">The row index (0–2).</param>
    /// <param name="column">The column index (0–2).</param>
    /// <returns>The float value at the specified (row, column) position.</returns>
    /// <exception cref="System.IndexOutOfRangeException">Thrown if either <paramref name="row"/> or <paramref name="column"/> is outside the range 0–2.</exception>
    public double this[int row, int column]
    {
        get => this[row * 3 + column];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorConversionMatrix"/> struct using individual float values.
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
    public ColorConversionMatrix(
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
    /// Multiplies a 3x3 matrix by a 3D vector.
    /// </summary>
    /// <param name="matrix">The 3x3 matrix.</param>
    /// <param name="colorXyz">The 3D vector.</param>
    /// <returns>The resulting 3D vector after multiplication.</returns>
    public static ColorXyz operator *(ColorConversionMatrix matrix, ColorXyz colorXyz)
    {
        return new
        (
            matrix.m00 * colorXyz.X + matrix.m01 * colorXyz.Y + matrix.m02 * colorXyz.Z,
            matrix.m10 * colorXyz.X + matrix.m11 * colorXyz.Y + matrix.m12 * colorXyz.Z,
            matrix.m20 * colorXyz.X + matrix.m21 * colorXyz.Y + matrix.m22 * colorXyz.Z
        );
    }

    /// <inheritdoc cref="operator *(ColorConversionMatrix, ColorXyz)"/>
    public static ColorXyz operator *(ColorXyz colorXyz, ColorConversionMatrix matrix)
    {
        return matrix * colorXyz;
    }

    /// <summary>
    /// Multiplies a 3x3 matrix by a LinearRgb vector.
    /// </summary>
    /// <param name="matrix">The 3x3 matrix.</param>
    /// <param name="linearRgb">The LinearRgb vector.</param>
    /// <returns>The resulting LinearRgb vector after multiplication.</returns>
    public static LinearRgb operator *(ColorConversionMatrix matrix, LinearRgb linearRgb)
    {
        return new
        (
            matrix.m00 * linearRgb.R + matrix.m01 * linearRgb.G + matrix.m02 * linearRgb.B,
            matrix.m10 * linearRgb.R + matrix.m11 * linearRgb.G + matrix.m12 * linearRgb.B,
            matrix.m20 * linearRgb.R + matrix.m21 * linearRgb.G + matrix.m22 * linearRgb.B
        );
    }

    /// <inheritdoc cref="operator *(ColorConversionMatrix, LinearRgb)"/>
    public static LinearRgb operator *(LinearRgb linearRgb, ColorConversionMatrix matrix)
    {
        return matrix * linearRgb;
    }

    /// <summary>
    /// Multiplies a 3x3 matrix by a Cam16Rgb vector.
    /// </summary>
    /// <param name="matrix">The 3x3 matrix.</param>
    /// <param name="cam16Rgb">The Cam16Rgb vector.</param>
    /// <returns>The resulting Cam16Rgb vector after multiplication.</returns>
    public static Cam16Rgb operator *(ColorConversionMatrix matrix, Cam16Rgb cam16Rgb)
    {
        return new
        (
            matrix.m00 * cam16Rgb.R + matrix.m01 * cam16Rgb.G + matrix.m02 * cam16Rgb.B,
            matrix.m10 * cam16Rgb.R + matrix.m11 * cam16Rgb.G + matrix.m12 * cam16Rgb.B,
            matrix.m20 * cam16Rgb.R + matrix.m21 * cam16Rgb.G + matrix.m22 * cam16Rgb.B
        );
    }

    /// <inheritdoc cref="operator *(ColorConversionMatrix, Cam16Rgb)"/>
    public static Cam16Rgb operator *(Cam16Rgb cam16Rgb, ColorConversionMatrix matrix)
    {
        return matrix * cam16Rgb;
    }

    /// <summary>
    /// Multiplies a 3x3 matrix by a ScaledDiscountRgb vector.
    /// </summary>
    /// <param name="matrix">The 3x3 matrix.</param>
    /// <param name="scaledDiscountRgb">The ScaledDiscountRgb vector.</param>
    /// <returns>The resulting ScaledDiscountRgb vector after multiplication.</returns>
    public static ScaledDiscountRgb operator *(ColorConversionMatrix matrix, ScaledDiscountRgb scaledDiscountRgb)
    {
        return new
        (
            matrix.m00 * scaledDiscountRgb.R + matrix.m01 * scaledDiscountRgb.G + matrix.m02 * scaledDiscountRgb.B,
            matrix.m10 * scaledDiscountRgb.R + matrix.m11 * scaledDiscountRgb.G + matrix.m12 * scaledDiscountRgb.B,
            matrix.m20 * scaledDiscountRgb.R + matrix.m21 * scaledDiscountRgb.G + matrix.m22 * scaledDiscountRgb.B
        );
    }

    /// <inheritdoc cref="operator *(ColorConversionMatrix, ScaledDiscountRgb)"/>
    public static ScaledDiscountRgb operator *(ScaledDiscountRgb scaledDiscountRgb, ColorConversionMatrix matrix)
    {
        return matrix * scaledDiscountRgb;
    }
}
