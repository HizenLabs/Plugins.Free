using HizenLabs.Extensions.UserPreference.Material.Constants;
using HizenLabs.Extensions.UserPreference.Material.Structs;
using HizenLabs.Extensions.UserPreference.Material.Utils;
using System;
using System.Runtime.CompilerServices;

namespace HizenLabs.Extensions.UserPreference.Material.ColorSpaces;

internal static class HctSolver
{
    public static StandardRgb SolveToColor(double hue, double chroma, double tone)
    {
        if (chroma < 0.0001 || tone < 0.0001 || tone > 99.9999)
        {
            return ColorUtils.ColorFromLstar(tone);
        }

        hue = MathUtils.SanitizeDegrees(hue);
        double hueRadians = hue / 180 * Math.PI;
        double y = ColorUtils.YFromLstar(tone);

        var exactAnswer = FindResultByJ(hueRadians, chroma, y);

        if (exactAnswer != 0)
        {
            return exactAnswer;
        }

        LinearRgb linrgb = BisectToLimit(y, hueRadians);

        return linrgb.ToStandardRgb();
    }

    static LinearRgb BisectToLimit(double y, double targetHue)
    {
        BisectToSegment(y, targetHue, out var left, out var right);

        double leftHue = HueOf(left);
        for (int axis = 0; axis < 3; axis++)
        {
            if (left[axis] != right[axis])
            {
                int lPlane, rPlane;
                if (left[axis] < right[axis])
                {
                    lPlane = CriticalPlaneBelow(ColorUtils.TrueDelinearized(left[axis]));
                    rPlane = CriticalPlaneAbove(ColorUtils.TrueDelinearized(right[axis]));
                }
                else
                {
                    lPlane = CriticalPlaneAbove(ColorUtils.TrueDelinearized(left[axis]));
                    rPlane = CriticalPlaneBelow(ColorUtils.TrueDelinearized(right[axis]));
                }

                for (int i = 0; i < 8; i++)
                {
                    if (Math.Abs(rPlane - lPlane) <= 1)
                    {
                        break;
                    }
                    else
                    {
                        int mPlane = (int)Math.Floor((lPlane + rPlane) / 2.0);
                        double midPlaneCoordinate = Perceptual.CriticalPlanes[mPlane];
                        Vector3d mid = SetCoordinate(left, midPlaneCoordinate, right, axis);
                        double midHue = HueOf(mid);
                        if (AreInCyclicOrder(leftHue, targetHue, midHue))
                        {
                            right = mid;
                            rPlane = mPlane;
                        }
                        else
                        {
                            left = mid;
                            leftHue = midHue;
                            lPlane = mPlane;
                        }
                    }
                }
            }
        }

        return Midpoint(left, right);
    }

    static StandardRgb FindResultByJ(double hueRadians, double chroma, double y)
    {
        // Initial estimate of j.
        double j = Math.Sqrt(y) * 11.0;
        // ===========================================================
        // Operations inlined from Cam16 to avoid repeated calculation
        // ===========================================================
        ViewingConditions viewingConditions = ViewingConditions.Default;
        double tInnerCoeff = 1 / Math.Pow(1.64 - Math.Pow(0.29, viewingConditions.N), 0.73);
        double eHue = 0.25 * (Math.Cos(hueRadians + 2.0) + 3.8);
        double p1 = eHue * (50000.0 / 13.0) * viewingConditions.Nc * viewingConditions.Ncb;
        double hSin = Math.Sin(hueRadians);
        double hCos = Math.Cos(hueRadians);
        for (int iterationRound = 0; iterationRound < 5; iterationRound++)
        {
            // ===========================================================
            // Operations inlined from Cam16 to avoid repeated calculation
            // ===========================================================
            double jNormalized = j / 100.0;
            double alpha = chroma == 0.0 || j == 0.0 ? 0.0 : chroma / Math.Sqrt(jNormalized);
            double t = Math.Pow(alpha * tInnerCoeff, 1.0 / 0.9);
            double ac =
                viewingConditions.Aw
                    * Math.Pow(jNormalized, 1.0 / viewingConditions.C / viewingConditions.Z);
            double p2 = ac / viewingConditions.Nbb;
            double gamma = 23.0 * (p2 + 0.305) * t / (23.0 * p1 + 11 * t * hCos + 108.0 * t * hSin);
            double a = gamma * hCos;
            double b = gamma * hSin;

            Cam16Rgb adapted = new
            (
                (460.0 * p2 + 451.0 * a + 288.0 * b) / 1403.0,
                (460.0 * p2 - 891.0 * a - 261.0 * b) / 1403.0,
                (460.0 * p2 - 220.0 * a - 6300.0 * b) / 1403.0
            );

            var scaledDiscount = ColorUtils.InverseChromaticAdaptationDiscount(adapted);

            var linrgb = scaledDiscount.ToLinearRgb();

            // ===========================================================
            // Operations inlined from Cam16 to avoid repeated calculation
            // ===========================================================
            if (linrgb.R < 0 || linrgb.G < 0 || linrgb.B < 0)
            {
                return 0;
            }

            var fnjVector = Gamma.YFromLinearRgb * linrgb;
            double fnj = fnjVector.Sum();

            if (fnj <= 0)
            {
                return 0;
            }
            if (iterationRound == 4 || Math.Abs(fnj - y) < 0.002)
            {
                if (linrgb.R > 100.01 || linrgb.G > 100.01 || linrgb.B > 100.01)
                {
                    return 0;
                }
                return linrgb.ToStandardRgb();
            }
            // Iterates with Newton method,
            // Using 2 * fn(j) / j as the approximation of fn'(j)
            j -= (fnj - y) * j / (2 * fnj);
        }
        return 0;
    }

    private static void BisectToSegment(double y, double targetHue, out Vector3d left, out Vector3d right)
    {
        left = new(-1.0, -1.0, -1.0);
        right = left;

        double leftHue = 0.0;
        double rightHue = 0.0;

        bool initialized = false;
        bool uncut = true;

        for (int n = 0; n < 12; n++)
        {
            Vector3d mid = NthVertex(y, n);
            if (mid.X < 0)
            {
                continue;
            }

            double midHue = HueOf(mid);
            if (!initialized)
            {
                left = mid;
                right = mid;
                leftHue = midHue;
                rightHue = midHue;
                initialized = true;
                continue;
            }
            if (uncut || AreInCyclicOrder(leftHue, midHue, rightHue))
            {
                uncut = false;
                if (AreInCyclicOrder(leftHue, targetHue, midHue))
                {
                    right = mid;
                    rightHue = midHue;
                }
                else
                {
                    left = mid;
                    leftHue = midHue;
                }
            }
        }
    }

    static Vector3d NthVertex(double y, int n)
    {
        double kR = Gamma.YFromLinearRgb.X;
        double kG = Gamma.YFromLinearRgb.Y;
        double kB = Gamma.YFromLinearRgb.Z;

        double coordA = n % 4 <= 1 ? 0.0 : 100.0;
        double coordB = n % 2 == 0 ? 0.0 : 100.0;
        if (n < 4)
        {
            double g = coordA;
            double b = coordB;
            double r = (y - g * kG - b * kB) / kR;
            if (IsBounded(r))
            {
                return new(r, g, b);
            }
            else
            {
                return new(-1.0, -1.0, -1.0);
            }
        }
        else if (n < 8)
        {
            double b = coordA;
            double r = coordB;
            double g = (y - r * kR - b * kB) / kG;
            if (IsBounded(g))
            {
                return new(r, g, b);
            }
            else
            {
                return new(-1.0, -1.0, -1.0);
            }
        }
        else
        {
            double r = coordA;
            double g = coordB;
            double b = (y - r * kR - g * kG) / kB;
            if (IsBounded(b))
            {
                return new(r, g, b);
            }
            else
            {
                return new(-1.0, -1.0, -1.0);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsBounded(double x)
    {
        return 0.0 <= x && x <= 100.0;
    }

    private static bool AreInCyclicOrder(double a, double b, double c)
    {
        var deltaAB = MathUtils.SanitizeRadians(b - a);
        var deltaAC = MathUtils.SanitizeRadians(c - a);

        return deltaAB < deltaAC;
    }

    private static double HueOf(LinearRgb linrgb)
    {
        var scaledDiscount = linrgb.ToScaledDiscount();

        var rgbA = ColorUtils.ChromaticAdaptationDiscount(scaledDiscount);
        var rA = rgbA.R;
        var gA = rgbA.G;
        var bA = rgbA.B;

        // redness-greenness
        double a = (11.0 * rA + -12.0 * gA + bA) / 11.0;

        // yellowness-blueness
        double b = (rA + gA - 2.0 * bA) / 9.0;

        return Math.Atan2(b, a);
    }

    private static Vector3d SetCoordinate(Vector3d source, double coordinate, Vector3d target, int axis)
    {
        double t = Intercept(source[axis], coordinate, target[axis]);
        
        return LerpPoint(source, t, target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double Intercept(double source, double mid, double target)
    {
        return (mid - source) / (target - source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector3d LerpPoint(Vector3d source, double t, Vector3d target)
    {
        return new
        (
            source[0] + (target[0] - source[0]) * t,
            source[1] + (target[1] - source[1]) * t,
            source[2] + (target[2] - source[2]) * t
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector3d Midpoint(Vector3d a, Vector3d b)
    {
        return new
        (
            (a.X + b.X) / 2,
            (a.Y + b.Y) / 2,
            (a.Z + b.Z) / 2
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CriticalPlaneBelow(double x)
    {
        return (int)Math.Floor(x - 0.5);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CriticalPlaneAbove(double x)
    {
        return (int)Math.Ceiling(x - 0.5);
    }
}
