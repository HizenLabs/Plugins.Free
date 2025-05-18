using Facepunch;
using HizenLabs.Extensions.UserPreference.Material.Enums;
using System;

namespace HizenLabs.Extensions.UserPreference.Material.DynamicColors;

internal static class ColorSpecs
{
	private static readonly ColorSpec2021 _spec2021 = new();

	public static IColorSpec Get(SpecVersion specVersion)
	{
		return specVersion switch
		{
			SpecVersion.SPEC_2021 => _spec2021,
            _ => throw new ArgumentOutOfRangeException(nameof(specVersion), specVersion, null)
		};
	}
}
