using Facepunch;
using HizenLabs.Extensions.UserPreference.Material.Enums;
using System;

namespace HizenLabs.Extensions.UserPreference.Material.DynamicColors;

public static class ColorSpecs
{
	public static IColorSpec Get(SpecVersion specVersion)
	{
		return specVersion switch
		{
			SpecVersion.SPEC_2021 => Pool.Get<ColorSpec2021>(),
			_ => throw new ArgumentOutOfRangeException(nameof(specVersion), specVersion, null)
		};
	}
}
